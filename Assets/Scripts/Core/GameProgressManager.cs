using System;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Serializable]
    public class LocationProgress
    {
        public LocationId locationId;
        public bool restored;
        public string memoryFragmentName;
    }

    [Header("Progress")]
    public int memoryFragmentsCollected;
    public bool busHubUnlocked;
    public bool endingUnlocked;

    [Header("Location States")]
    public LocationProgress[] locationStates =
    {
        new LocationProgress { locationId = LocationId.NguyenHue, memoryFragmentName = "Nhịp sống trẻ" },
        new LocationProgress { locationId = LocationId.BenThanh, memoryFragmentName = "Đời sống thường ngày" },
        new LocationProgress { locationId = LocationId.DinhDocLap, memoryFragmentName = "Lịch sử" },
        new LocationProgress { locationId = LocationId.NhaThoDucBa, memoryFragmentName = "Bình yên" },
        new LocationProgress { locationId = LocationId.Bitexco, memoryFragmentName = "Chuyển mình" },
        new LocationProgress { locationId = LocationId.BachDang, memoryFragmentName = "Dòng chảy thành phố" }
    };

    private readonly List<string> collectedFragments = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RecalculateProgress();
    }

    public bool IsRestored(LocationId locationId)
    {
        LocationProgress progress = GetLocationProgress(locationId);
        return progress != null && progress.restored;
    }

    public void MarkLocationRestored(LocationId locationId, string fragmentName)
    {
        LocationProgress progress = GetLocationProgress(locationId);
        if (progress == null || progress.restored)
        {
            return;
        }

        progress.restored = true;
        progress.memoryFragmentName = fragmentName;

        if (!collectedFragments.Contains(fragmentName))
        {
            collectedFragments.Add(fragmentName);
        }

        if (locationId == LocationId.NguyenHue)
        {
            busHubUnlocked = true;
        }

        RecalculateProgress();
        UIManager.Instance?.RefreshProgressText();
    }

    public IReadOnlyList<string> GetCollectedFragments()
    {
        return collectedFragments;
    }

    public void ResetProgressForTesting()
    {
        foreach (LocationProgress progress in locationStates)
        {
            progress.restored = false;
        }

        collectedFragments.Clear();
        memoryFragmentsCollected = 0;
        busHubUnlocked = false;
        endingUnlocked = false;
        UIManager.Instance?.RefreshProgressText();
    }

    private LocationProgress GetLocationProgress(LocationId locationId)
    {
        foreach (LocationProgress progress in locationStates)
        {
            if (progress.locationId == locationId)
            {
                return progress;
            }
        }

        return null;
    }

    private void RecalculateProgress()
    {
        memoryFragmentsCollected = 0;
        collectedFragments.Clear();

        foreach (LocationProgress progress in locationStates)
        {
            if (progress.restored)
            {
                memoryFragmentsCollected++;
                if (!string.IsNullOrWhiteSpace(progress.memoryFragmentName))
                {
                    collectedFragments.Add(progress.memoryFragmentName);
                }
            }
        }

        endingUnlocked = memoryFragmentsCollected >= 6;
    }
}
