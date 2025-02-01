using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using JammerDash;
using System;

namespace JammerDash.Menus.Play {
    public class LevelSearch : MonoBehaviour
{
    public Transform contentParent; // The parent object containing the level prefabs
    public InputField searchField;
    public Button sortByNameButton;
    public Button sortByMapperButton;

    private bool isNameAscending = true;
    private bool isMapperAscending = true;

    private List<CustomLevelScript> levelScripts = new List<CustomLevelScript>();

    private void Start()
    {
        // Collect all CustomLevelScript components
        foreach (Transform child in contentParent)
        {
            var levelScript = child.GetComponent<CustomLevelScript>();
            if (levelScript != null)
            {
                levelScripts.Add(levelScript);
            }
        }

        // Add listeners
        sortByNameButton.onClick.AddListener(SortByName);
        sortByMapperButton.onClick.AddListener(SortByMapper);
        searchField.onValueChanged.AddListener(SearchAndFilter);
    }

      private void SortByName()
    {
        levelScripts = isNameAscending
            ? levelScripts.OrderBy(l => l.sceneData.name).ToList()
            : levelScripts.OrderByDescending(l => l.sceneData.name).ToList();

        isNameAscending = !isMapperAscending;
        isMapperAscending = false;
        RefreshUI();
    }

    private void SortByMapper()
    {
        levelScripts = isMapperAscending
            ? levelScripts.OrderBy(l => l.sceneData.creator).ToList()
            : levelScripts.OrderByDescending(l => l.sceneData.creator).ToList();

        isNameAscending = false;
        isMapperAscending = !isMapperAscending;
        RefreshUI();
    }

    private void SearchAndFilter(string query)
    {
        // Parse the query for filters
        string[] filters = query.Split(';');
        var filteredLevels = levelScripts;

        foreach (var filter in filters)
        {
            if (filter.StartsWith("artist="))
            {
                string artist = filter[7..].ToLower();
                filteredLevels = filteredLevels.Where(l => l.sceneData.artist.ToLower().Contains(artist)).ToList();
            }
            else if (filter.StartsWith("mapper="))
            {
                string mapper = filter[7..].ToLower();
                filteredLevels = filteredLevels.Where(l => l.sceneData.creator.ToLower().Contains(mapper)).ToList();
            }
            else if (filter.StartsWith("shine="))
            {
                if (float.TryParse(filter[6..], out float shineValue))
                {
                    filteredLevels = filteredLevels.Where(l => Mathf.Floor(l.sceneData.calculatedDifficulty) == Mathf.Floor(shineValue)).ToList();
                }
            }
            else
            {
                string name = filter.ToLower();
                filteredLevels = filteredLevels.Where(l => l.sceneData.name.ToLower().Contains(name)).ToList();
           
            }
        }

        // Refresh the UI with filtered levels
        RefreshUI(filteredLevels);
    }

    private void RefreshUI(List<CustomLevelScript> filteredLevels = null)
{
    // Default to all levels if no specific filtered list is provided
    filteredLevels ??= levelScripts;

    // Rearrange the GameObjects in the hierarchy
    for (int i = 0; i < filteredLevels.Count; i++)
    {
        filteredLevels[i].transform.SetSiblingIndex(i);
    }

    // Activate only the filtered levels
    foreach (var levelScript in levelScripts)
    {
        levelScript.gameObject.SetActive(filteredLevels.Contains(levelScript));
        levelScript.GetComponent<ButtonClickHandler>().isSelected = false;
    }
}

}

}
