using UnityEngine;
using System.Collections.Generic;


public class ArtAssessmentManager : MonoBehaviour
{
    public static ArtAssessmentManager Instance;

    [Header("Story Configuration")]
    public StoryMapping storyMapping;

    [Header("References")]
    public BoardManager boardManager;

    private float designStartTime;
    private float designEndTime;
    private bool isTracking = false;

    private int skyPlaceCount = 0;
    private bool skyFirstWasCorrect = false;
    private float firstSkyTime = -1f;

    private int buildingPlaceCount = 0;
    private bool buildingFirstWasCorrect = false;
    private float firstBuildingTime = -1f;

    private int characterPlaceCount = 0;
    private bool characterFirstWasCorrect = false;
    private float firstCharacterTime = -1f;

    private Dictionary<string, int> decorPlaceCounts = new Dictionary<string, int>();
    private float firstDecorTime = -1f;

    private Dictionary<string, int> colorApplyCounts = new Dictionary<string, int>();

    private string lastSelectedTitle = "";
    private int titleAttemptCount = 0;
    private bool titleFirstWasCorrect = false;
    private float titleFirstTime = -1f;
    private float titleQuestionOpenTime = -1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartTracking()
    {
        ResetAll();
        designStartTime = Time.time;
        isTracking = true;

        PsychometricReportManager.Instance?.SetupNewAspect("الإخراج الفني", "Game_3");
    }

    public void StopTracking()
    {
        if (!isTracking) return;
        isTracking = false;
        designEndTime = Time.time;
        Debug.Log($"[Art] انتهى التصميم | المدة: {designEndTime - designStartTime:F1}s");
    }

    private void ResetAll()
    {
        skyPlaceCount = 0; skyFirstWasCorrect = false; firstSkyTime = -1f;
        buildingPlaceCount = 0; buildingFirstWasCorrect = false; firstBuildingTime = -1f;
        characterPlaceCount = 0; characterFirstWasCorrect = false; firstCharacterTime = -1f;
        decorPlaceCounts.Clear();
        firstDecorTime = -1f;
        colorApplyCounts.Clear();
        lastSelectedTitle = "";
        titleAttemptCount = 0;
        titleFirstWasCorrect = false;
        titleFirstTime = -1f;
        titleQuestionOpenTime = -1f;
    }

    public void OnItemPlaced(GameObject item, string prefabName, Vector2 pos)
    {
        if (!isTracking || storyMapping == null) return;

        AssessmentTag tag = item.GetComponent<AssessmentTag>();
        if (tag == null)
        {
            Debug.LogWarning($"[Art] عنصر بدون AssessmentTag: {item.name}");
            return;
        }

        float now = Time.time - designStartTime;

        switch (tag.category)
        {
            case "BG":
                if (firstSkyTime < 0f) firstSkyTime = now;
                if (skyPlaceCount == 0)
                    skyFirstWasCorrect = storyMapping.allowedSky.Contains(prefabName);
                skyPlaceCount++;
                Debug.Log($"[Art][BG] #{skyPlaceCount}: {prefabName} | صح={storyMapping.allowedSky.Contains(prefabName)}");
                break;

            case "Building":
                if (firstBuildingTime < 0f) firstBuildingTime = now;
                if (buildingPlaceCount == 0)
                    buildingFirstWasCorrect = storyMapping.allowedBuildings.Contains(prefabName);
                buildingPlaceCount++;
                Debug.Log($"[Art][Building] #{buildingPlaceCount}: {prefabName} | صح={storyMapping.allowedBuildings.Contains(prefabName)}");
                break;

            case "Character":
                if (firstCharacterTime < 0f) firstCharacterTime = now;
                if (characterPlaceCount == 0)
                    characterFirstWasCorrect = storyMapping.allowedCharacters.Contains(prefabName);
                characterPlaceCount++;
                Debug.Log($"[Art][Character] #{characterPlaceCount}: {prefabName} | صح={storyMapping.allowedCharacters.Contains(prefabName)}");
                break;

            case "Decoration":
                if (firstDecorTime < 0f) firstDecorTime = now;
                if (!decorPlaceCounts.ContainsKey(prefabName))
                    decorPlaceCounts[prefabName] = 0;
                decorPlaceCounts[prefabName]++;
                Debug.Log($"[Art][Decor] {prefabName} | مرة #{decorPlaceCounts[prefabName]}");
                break;

            default:
                Debug.LogWarning($"[Art] category غير معروف: {tag.category}");
                break;
        }
    }

    public void OnItemDeleted(GameObject item)
    {
        if (!isTracking || item == null) return;
        AssessmentTag tag = item.GetComponent<AssessmentTag>();
        Debug.Log($"[Art][Delete] حُذف: {item.name} | category={tag?.category}");
    }

    public void OnColorApplied(string colorName)
    {
        if (!isTracking) return;
        string c = colorName.ToLower();
        if (!colorApplyCounts.ContainsKey(c)) colorApplyCounts[c] = 0;
        colorApplyCounts[c]++;
        Debug.Log($"[Art][Color] {colorName} | إجمالي استخدام هذا اللون: {colorApplyCounts[c]}");
    }

    public void OnTitleSelected(string titleId)
    {
        if (titleQuestionOpenTime < 0) return;

        float now = Time.time - designStartTime;
        bool isCorrect = titleId == storyMapping?.correctTitleId;

        titleAttemptCount++;
        if (titleAttemptCount == 1)
        {
            titleFirstWasCorrect = isCorrect;
            titleFirstTime = now;
        }

        lastSelectedTitle = titleId;
        Debug.Log($"[Art][Title] #{titleAttemptCount}: {titleId} | صح={isCorrect}");
    }

    public void StartTitleTimer()
    {
        titleQuestionOpenTime = Time.time;
        Debug.Log("[Art] بدأ توقيت السؤال اللفظي");
    }

    public void OnConfirm()
    {
        CalculateAll();
    }

    private void CalculateAll()
    {
        if (storyMapping == null)
        {
            return;
        }
        if (boardManager == null)
        {
            return;
        }

        string finalSky = "";
        string finalBuilding = "";
        string finalCharacter = "";
        HashSet<string> finalDecorNames = new HashSet<string>();
        Dictionary<string, int> finalDecorTypeCounts = new Dictionary<string, int>();
        List<Vector2> allPositions = new List<Vector2>();
        HashSet<string> usedLayerCategories = new HashSet<string>();

        ReadLayer(boardManager.bgLayer, "BG",
            ref finalSky, ref finalBuilding, ref finalCharacter,
            finalDecorNames, finalDecorTypeCounts, allPositions, usedLayerCategories);

        ReadLayer(boardManager.buildingLayer, "Building",
            ref finalSky, ref finalBuilding, ref finalCharacter,
            finalDecorNames, finalDecorTypeCounts, allPositions, usedLayerCategories);

        ReadLayer(boardManager.charLayer, "Character",
            ref finalSky, ref finalBuilding, ref finalCharacter,
            finalDecorNames, finalDecorTypeCounts, allPositions, usedLayerCategories);

        ReadLayer(boardManager.decoLayer, "Decoration",
            ref finalSky, ref finalBuilding, ref finalCharacter,
            finalDecorNames, finalDecorTypeCounts, allPositions, usedLayerCategories);

        HashSet<string> uniqueColorsUsed = new HashSet<string>(colorApplyCounts.Keys);

        float totalDesignTime = designEndTime - designStartTime;

        float shapesAnchor = Mathf.Min(
            firstSkyTime >= 0 ? firstSkyTime : totalDesignTime,
            firstBuildingTime >= 0 ? firstBuildingTime : totalDesignTime
        );
        float shapesTimeTaken = totalDesignTime - shapesAnchor;
        if (shapesTimeTaken <= 0f) shapesTimeTaken = totalDesignTime;

        float symbolsAnchor = Mathf.Min(
            firstCharacterTime >= 0 ? firstCharacterTime : totalDesignTime,
            firstDecorTime >= 0 ? firstDecorTime : totalDesignTime
        );
        float symbolsTimeTaken = totalDesignTime - symbolsAnchor;
        if (symbolsTimeTaken <= 0f) symbolsTimeTaken = totalDesignTime;

        Debug.Log($"[Art] الحالة النهائية → sky={finalSky} | bld={finalBuilding} | char={finalCharacter} | decor={finalDecorNames.Count} | colors={uniqueColorsUsed.Count}");
        Debug.Log($"[Art] الأوقات → total={totalDesignTime:F1}s | shapes={shapesTimeTaken:F1}s | symbols={symbolsTimeTaken:F1}s");

        SubmitInd1_Shapes(finalSky, finalBuilding, shapesTimeTaken);
        SubmitInd2_Colors(uniqueColorsUsed);
        SubmitInd3_CharactersAndSymbols(finalCharacter, finalDecorNames, symbolsTimeTaken);
        SubmitInd4_Decorations(finalDecorNames);
        SubmitInd5_OverallCoherence(finalSky, finalBuilding, finalCharacter, finalDecorNames, uniqueColorsUsed);
        SubmitInd6_VisualBalance(usedLayerCategories, finalDecorTypeCounts, allPositions);
        SubmitInd7_VerbalAwareness();
        SubmitInd8_ElementDiversity(usedLayerCategories, finalDecorNames);
        if (LocalProgressManager.Instance != null)
        {
            LocalProgressManager.Instance.MarkGameComplete(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        PsychometricReportManager.Instance?.UploadCurrentGameResult();
        Debug.Log("[Art] ══ اكتمل القياس ══");
    }

    private void ReadLayer(Transform layer, string category,
        ref string sky, ref string building, ref string character,
        HashSet<string> decorNames,
        Dictionary<string, int> decorTypeCounts,
        List<Vector2> positions,
        HashSet<string> usedLayers)
    {
        if (layer == null) return;

        foreach (Transform child in layer)
        {
            AssessmentTag childTag = child.GetComponent<AssessmentTag>();
            string prefabName = (childTag != null && !string.IsNullOrEmpty(childTag.itemId))
                ? childTag.itemId
                : child.gameObject.name.Replace("(Clone)", "").Trim();

            RectTransform rt = child.GetComponent<RectTransform>();
            if (rt != null) positions.Add(rt.anchoredPosition);

            switch (category)
            {
                case "BG":
                    sky = prefabName;
                    if (!string.IsNullOrEmpty(prefabName)) usedLayers.Add("BG");
                    break;

                case "Building":
                    building = prefabName;
                    if (!string.IsNullOrEmpty(prefabName)) usedLayers.Add("Building");
                    break;

                case "Character":
                    character = prefabName;
                    if (!string.IsNullOrEmpty(prefabName)) usedLayers.Add("Character");
                    break;

                case "Decoration":
                    decorNames.Add(prefabName);
                    usedLayers.Add("Decoration");

                    string decorType = prefabName.Length > 2 ? prefabName.Substring(2) : prefabName;
                    if (decorTypeCounts.ContainsKey(decorType)) decorTypeCounts[decorType]++;
                    else decorTypeCounts[decorType] = 1;
                    break;
            }
        }
    }

    private void SubmitInd1_Shapes(string sky, string building, float timeTaken)
    {
        bool skyCorrect = !string.IsNullOrEmpty(sky) && storyMapping.allowedSky.Contains(sky);
        bool buildingCorrect = !string.IsNullOrEmpty(building) && storyMapping.allowedBuildings.Contains(building);

        float accuracy = ((skyCorrect ? 1 : 0) + (buildingCorrect ? 1 : 0)) / 2f;

        float speed = Mathf.Clamp01(storyMapping.standardTimeShapes / Mathf.Max(timeTaken, 0.1f));

        float firstAttempt = ((skyFirstWasCorrect ? 1 : 0) + (buildingFirstWasCorrect ? 1 : 0)) / 2f;

        float score = (accuracy * 0.6f) + (speed * 0.2f) + (firstAttempt * 0.2f);

        Debug.Log($"[Ind1] sky={skyCorrect}(first={skyFirstWasCorrect}) bld={buildingCorrect}(first={buildingFirstWasCorrect}) | acc={accuracy:F2} spd={speed:F2} fa={firstAttempt:F2} → {score:F2}");
        BuildAndSubmit(
            "اختيار أشكال أساسية ترمز للشعور",
            "يختار الخلفية والمبنى المناسبين للشعور دون تردد أو خلط بين الخيارات.",
            score);
    }

    private void SubmitInd2_Colors(HashSet<string> uniqueColorsUsed)
    {
        if (uniqueColorsUsed.Count == 0)
        {
            BuildAndSubmit(
                "توظيف الألوان كسمة مجردة عن الشعور",
                "يستخدم ألوانًا متسقة مع الشعور ويختار الدرجات المناسبة.",
                0.3f);
            return;
        }

        if (storyMapping.paletteMood == null || storyMapping.paletteMood.Count == 0)
        {
            BuildAndSubmit(
                "توظيف الألوان كسمة مجردة عن الشعور",
                "يستخدم ألوانًا متسقة مع الشعور ويختار الدرجات المناسبة.",
                0f);
            return;
        }

        int appropriateColors = 0;
        foreach (string c in uniqueColorsUsed)
            if (storyMapping.paletteMood.Contains(c)) appropriateColors++;

        float score = (float)appropriateColors / uniqueColorsUsed.Count;
        score = Mathf.Clamp01(score);

        Debug.Log($"[Ind2] ألوان مستخدمة={uniqueColorsUsed.Count} | مناسبة={appropriateColors} → {score:F2}");
        BuildAndSubmit(
            "توظيف الألوان كسمة مجردة عن الشعور",
            "يستخدم ألوانًا متسقة مع الشعور ويختار الدرجات المناسبة.",
            score);
    }

    private void SubmitInd3_CharactersAndSymbols(string character, HashSet<string> finalDecorNames, float timeTaken)
    {
        bool charCorrect = !string.IsNullOrEmpty(character) && storyMapping.allowedCharacters.Contains(character);
        int N = 1 + storyMapping.expectedSymbolsCount;

        int correctSymbols = 0;
        foreach (string sym in finalDecorNames)
            if (storyMapping.allowedSymbols.Contains(sym)) correctSymbols++;

        float accuracy = Mathf.Clamp01(((charCorrect ? 1 : 0) + correctSymbols) / (float)N);

        float speed = Mathf.Clamp01(storyMapping.standardTimeSymbols / Mathf.Max(timeTaken, 0.1f));

        int firstAttemptCorrect = characterFirstWasCorrect ? 1 : 0;
        foreach (string sym in finalDecorNames)
        {
            if (storyMapping.allowedSymbols.Contains(sym) &&
                decorPlaceCounts.TryGetValue(sym, out int count) && count == 1)
                firstAttemptCorrect++;
        }
        float firstAttemptRatio = Mathf.Clamp01((float)firstAttemptCorrect / N);

        float score = (accuracy * 0.6f) + (speed * 0.2f) + (firstAttemptRatio * 0.2f);

        Debug.Log($"[Ind3] char={charCorrect}(first={characterFirstWasCorrect}) symbols={correctSymbols}/{storyMapping.expectedSymbolsCount} | acc={accuracy:F2} spd={speed:F2} fa={firstAttemptRatio:F2} → {score:F2}");
        BuildAndSubmit(
            "اختيار الملامح والرموز التي تعبر عن الشعور",
            "يختار الشخصيات والرموز التي تعكس الشعور بدقة، ويميز بين الملامح دون أخطاء.",
            score);
    }

    private void SubmitInd4_Decorations(HashSet<string> finalDecorNames)
    {
        int maxAvail = storyMapping.maxDecorAvailable > 0 ? storyMapping.maxDecorAvailable : 8;

        float weightedDecor = 0f;
        int appropriateCount = 0;
        foreach (string d in finalDecorNames)
        {
            if (storyMapping.allowedSymbols.Contains(d))
            {
                weightedDecor += 1.0f;
                appropriateCount++;
            }
            else if (d.Length > 2 && d.StartsWith("N_"))
            {
                weightedDecor += 0.5f;
            }
        }

        float quantityScore = Mathf.Clamp01(weightedDecor / maxAvail);

        float qualityScore = finalDecorNames.Count > 0
            ? Mathf.Clamp01(weightedDecor / finalDecorNames.Count)
            : 0f;

        float score = (quantityScore * 0.6f) + (qualityScore * 0.4f);

        Debug.Log($"[Ind4] زينة كلية={finalDecorNames.Count} | موزونة={weightedDecor:F1}/{maxAvail} | qty={quantityScore:F2} qual={qualityScore:F2} → {score:F2}");
        BuildAndSubmit(
            "إضافة عناصر زينة إضافية لتجميل المشهد",
            "يضيف عناصر زينة تدعم جمالية المشهد ويختارها بوعي دون إفراط أو إهمال.",
            score);
    }

    private void SubmitInd5_OverallCoherence(string sky, string building, string character,
        HashSet<string> finalDecorNames, HashSet<string> uniqueColorsUsed)
    {
        float mSky = (!string.IsNullOrEmpty(sky) && storyMapping.allowedSky.Contains(sky)) ? 1f : 0f;
        float mBuilding = (!string.IsNullOrEmpty(building) && storyMapping.allowedBuildings.Contains(building)) ? 1f : 0f;
        float mChar = (!string.IsNullOrEmpty(character) && storyMapping.allowedCharacters.Contains(character)) ? 1f : 0f;

        int correctDecor = 0;
        foreach (string d in finalDecorNames)
            if (storyMapping.allowedSymbols.Contains(d)) correctDecor++;
        float mDecor = finalDecorNames.Count > 0
            ? Mathf.Clamp01((float)correctDecor / finalDecorNames.Count)
            : 0f;

        int correctColors = 0;
        foreach (string c in uniqueColorsUsed)
            if (storyMapping.paletteMood.Contains(c)) correctColors++;
        float mColors = uniqueColorsUsed.Count > 0
            ? Mathf.Clamp01((float)correctColors / uniqueColorsUsed.Count)
            : 0f;

        float score = (mSky * 0.2f) + (mBuilding * 0.2f) + (mChar * 0.2f) + (mDecor * 0.25f) + (mColors * 0.15f);

        Debug.Log($"[Ind5] sky={mSky} bld={mBuilding} chr={mChar} decor={mDecor:F2} clr={mColors:F2} → {score:F2}");
        BuildAndSubmit(
            "الوصول إلى لوحة متكاملة تمثل الشعور",
            "ينسّق العناصر المختارة بشكل يحقق انسجامًا بصريًا واضحًا يعكس الشعور المطلوب.",
            score);
    }

    private void SubmitInd6_VisualBalance(HashSet<string> usedLayerCategories,
        Dictionary<string, int> decorTypeCounts, List<Vector2> allPositions)
    {
        float coverage = usedLayerCategories.Count / 4f;

        float evenness = CalcEvenness(decorTypeCounts);

        float spread = CalcSpread(allPositions);

        Debug.Log($"[Ind6] coverage={coverage:F2} evenness={evenness:F2} spread={spread:F2}");

        var pm = PsychometricReportManager.Instance;
        if (pm == null) return;

        pm.StartNewIndicator("تحقيق توازن بصري من خلال اختيار العناصر والألوان بشكل متسق");
        AddItem(pm, 1, "يوزع اختياراته عبر فئات بصرية متعددة بدلًا من الاعتماد على فئة واحدة فقط.", coverage);
        AddItem(pm, 2, "يوزع عناصر الزينة بين أنواع مختلفة دون تركيزها في نوع واحد.", evenness);
        AddItem(pm, 3, "ينشر العناصر في مناطق متعددة من اللوحة.", spread);
        pm.FinishCurrentIndicator();
    }

    private void SubmitInd7_VerbalAwareness()
    {
        if (titleAttemptCount == 0)
        {
            Debug.Log("[Ind7] لم يختر عنواناً → 0.00");
            BuildAndSubmit(
                "التوافق بين الإنتاج البصري والوعي اللفظي تجاه لوحته",
                "يختار اسمًا يعكس الشعور الذي تمثله اللوحة بدقة ووضوح.",
                0f);
            return;
        }

        bool finalCorrect = !string.IsNullOrEmpty(lastSelectedTitle) &&
                            lastSelectedTitle == storyMapping.correctTitleId;

        float accuracy = finalCorrect ? 1f : 0f;

        float standardTime = storyMapping.standardTimeSymbols > 0 ? storyMapping.standardTimeSymbols : 15f;
        float timeTaken = standardTime;
        if (titleQuestionOpenTime >= 0 && titleFirstTime >= 0)
        {

            float questionOpenRelative = titleQuestionOpenTime - designStartTime;
            timeTaken = Mathf.Max(titleFirstTime - questionOpenRelative, 0.1f);
        }
        float speed = Mathf.Clamp01(standardTime / timeTaken);

        float errorRate = finalCorrect ? Mathf.Clamp01(1f / titleAttemptCount) : 0f;

        float score = (accuracy * 0.6f) + (speed * 0.2f) + (errorRate * 0.2f);

        Debug.Log($"[Ind7] title='{lastSelectedTitle}' attempts={titleAttemptCount} firstCorrect={titleFirstWasCorrect} | acc={accuracy:F2} spd={speed:F2} err={errorRate:F2} → {score:F2}");
        BuildAndSubmit(
            "التوافق بين الإنتاج البصري والوعي اللفظي تجاه لوحته",
            "يختار اسمًا يعكس الشعور الذي تمثله اللوحة بدقة ووضوح.",
            score);
    }

    private void SubmitInd8_ElementDiversity(HashSet<string> usedLayerCategories,
        HashSet<string> finalDecorNames)
    {
        float categoryDiversity = usedLayerCategories.Count / 4f;

        HashSet<string> uniqueDecorTypes = new HashSet<string>();
        foreach (string d in finalDecorNames)
        {
            string t = d.Length > 2 ? d.Substring(2) : d;
            uniqueDecorTypes.Add(t);
        }
        int maxExpectedDecorTypes = Mathf.Max(storyMapping.maxDecorAvailable, 1);
        float decorDiversity = Mathf.Clamp01((float)uniqueDecorTypes.Count / maxExpectedDecorTypes);

        float score = (categoryDiversity * 0.5f) + (decorDiversity * 0.5f);

        Debug.Log($"[Ind8] فئات={usedLayerCategories.Count}/4 | أنواع زينة={uniqueDecorTypes.Count} → {score:F2}");
        BuildAndSubmit(
            "اختيار عناصر متنوعة من فئات مختلفة",
            "يوظّف عناصر من فئات متعددة لبناء مشهد متكامل دون الاعتماد على فئة واحدة.",
            score);
    }

    private float CalcEvenness(Dictionary<string, int> counts)
    {
        if (counts == null || counts.Count == 0) return 0f;
        if (counts.Count == 1) return 0f;

        int total = 0;
        foreach (var v in counts.Values) total += v;
        if (total == 0) return 0f;

        int m = counts.Count;
        float mean = 1f / m;
        float variance = 0f;
        foreach (var v in counts.Values)
        {
            float q = v / (float)total;
            variance += (q - mean) * (q - mean);
        }
        float sigma = Mathf.Sqrt(variance / m);

        float sigmaMax = Mathf.Sqrt(((1f - mean) * (1f - mean) + (m - 1) * mean * mean) / m);
        if (sigmaMax <= 0f) return 1f;

        return Mathf.Clamp01(1f - sigma / sigmaMax);
    }

    private float CalcSpread(List<Vector2> positions)
    {
        if (positions == null || positions.Count < 2) return 0f;
        if (boardManager?.drawingArea == null) return 0f;

        Rect area = boardManager.drawingArea.rect;
        float areaSize = area.width * area.height;
        if (areaSize <= 0f) return 0f;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector2 p in positions)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        float occupiedArea = Mathf.Max(maxX - minX, 1f) * Mathf.Max(maxY - minY, 1f);
        float threshold = areaSize * Mathf.Clamp01(storyMapping.areaThresholdRatio);
        if (threshold <= 0f) threshold = areaSize * 0.6f;

        return Mathf.Clamp01(occupiedArea / threshold);
    }

    private void BuildAndSubmit(string indicatorName, string itemName, float score)
    {
        var pm = PsychometricReportManager.Instance;
        if (pm == null) return;
        pm.StartNewIndicator(indicatorName);
        AddItem(pm, 1, itemName, score);
        pm.FinishCurrentIndicator();
    }

    private void AddItem(PsychometricReportManager pm, int index, string name, float score)
    {
        score = Mathf.Clamp01(score);
        var item = new AssessmentItemDto
        {
            itemIndex = index,
            itemName = name,
            finalScore = score
        };
        if (score >= 0.80f) { item.rating = "غالبًا"; item.psychometricPts = 3; }
        else if (score >= 0.50f) { item.rating = "أحيانًا"; item.psychometricPts = 2; }
        else { item.rating = "نادرًا"; item.psychometricPts = 1; }

        pm.currentIndicator?.items.Add(item);
    }

    void OnApplicationQuit()
    {
        if (isTracking)
        {
            StopTracking();
            CalculateAll();
        }
    }
}