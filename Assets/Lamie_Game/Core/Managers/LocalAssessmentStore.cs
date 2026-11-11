using System;

// Small persistence boundary: policy can be tested without Unity or a server.
public interface ILocalAssessmentPersistence
{
    string Read(string key);
    void Write(string key, string value);
    void Save();
}

public sealed class LocalAssessmentStore
{
    private readonly ILocalAssessmentPersistence persistence;
    public LocalAssessmentStore(ILocalAssessmentPersistence persistence)
    {
        this.persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    private static string Key(string child, string scene, string field)
    {
        if (string.IsNullOrWhiteSpace(child) || string.IsNullOrWhiteSpace(scene))
            throw new ArgumentException("Assessment storage requires an identified child and game.");
        return "lamie_assessment_v1_" + Uri.EscapeDataString(child) + "_" +
            Uri.EscapeDataString(scene) + "_" + field;
    }

    public bool HasBaseline(string child, string scene) =>
        !string.IsNullOrEmpty(persistence.Read(Key(child, scene, "baseline")));
    public string ReadBaseline(string child, string scene) => persistence.Read(Key(child, scene, "baseline"));
    public string ReadDelivery(string child, string scene) => persistence.Read(Key(child, scene, "delivery"));
    public int ReadAttemptCount(string child, string scene)
    {
        int.TryParse(persistence.Read(Key(child, scene, "attempts")), out int count);
        return count;
    }

    // Nothing from an interrupted attempt is resumed. Only its exposure count survives.
    public int BeginAttempt(string child, string scene)
    {
        if (HasBaseline(child, scene)) return 0; // Practice must never modify baseline metadata.
        int count = ReadAttemptCount(child, scene) + 1;
        persistence.Write(Key(child, scene, "attempts"), count.ToString(System.Globalization.CultureInfo.InvariantCulture));
        persistence.Save();
        return count;
    }

    public bool TrySaveFirstCompletion(string child, string scene, string record)
    {
        if (string.IsNullOrEmpty(record)) throw new ArgumentException("Completed record is empty.");
        if (HasBaseline(child, scene)) return false;
        persistence.Write(Key(child, scene, "baseline"), record);
        persistence.Write(Key(child, scene, "delivery"), "pending");
        persistence.Save();
        return true;
    }

    // Persist before sending. An ambiguous response must not be retried automatically
    // against the existing API, which has no idempotency key support.
    public bool TryBeginDelivery(string child, string scene)
    {
        if (!HasBaseline(child, scene) || ReadDelivery(child, scene) != "pending") return false;
        persistence.Write(Key(child, scene, "delivery"), "unconfirmed");
        persistence.Save();
        return true;
    }

    public void ConfirmDelivery(string child, string scene)
    {
        if (!HasBaseline(child, scene)) return;
        persistence.Write(Key(child, scene, "delivery"), "sent");
        persistence.Save();
    }

    public void RejectDelivery(string child, string scene)
    {
        if (!HasBaseline(child, scene)) return;
        persistence.Write(Key(child, scene, "delivery"), "pending");
        persistence.Save();
    }
}
