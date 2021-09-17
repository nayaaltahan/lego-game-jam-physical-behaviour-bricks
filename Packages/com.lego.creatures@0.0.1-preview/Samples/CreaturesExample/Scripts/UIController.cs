using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LEGO.Creatures.Sample
{
    public class UIController : MonoBehaviour
    {
        public Dropdown CreatureSelector = default;

        public GameObject templateButton;

        List<CreatureController> connectedCreatures = new List<CreatureController>();
        List<GameObject> currentButtons = new List<GameObject>();

        Transform animationContent;

        CameraController cameraFocus;

        bool raycastEnabled = true;
        int currentIndex = -1;

        void Awake()
        {
            cameraFocus = Camera.main.gameObject.GetComponent<CameraController>();
        }

        void Start()
        {
            animationContent = GameObject.Find("Animation UI/Animation Scroll View/Viewport/Content").transform;

            if (CreatureSelector)
            {
                PopulateDropdowns();
                ToggleButtons();
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && raycastEnabled)
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    var objectHit = hit.transform.gameObject;
                    var creatureIndex = connectedCreatures.FindIndex(creature => creature.gameObject == objectHit);

                    if (creatureIndex >= 0 && currentIndex != creatureIndex)
                    {
                        currentIndex = creatureIndex;

                        CreatureSelector.value = creatureIndex + 1;
                        ShowCreatureMenu();

                        cameraFocus?.FocusCamera(objectHit);
                    }
                }
            }
        }

        public void ShowCreatureMenu()
        {
            if (CreatureSelector)
            {
                if (CreatureSelector.value == 0)
                {
                    cameraFocus?.ResetPosition();
                }
                else
                {
                    cameraFocus?.FocusCamera(connectedCreatures[CreatureSelector.value - 1].gameObject);
                }

                currentIndex = CreatureSelector.value - 1;

                RefreshButtons();
            }
        }

        public void EnableRaycastObjects(bool enable)
        {
            raycastEnabled = enable; // Buttons set this on pointer enter/exit to prevent raycasts from selecting objects behind.
        }

        void ToggleButtons()
        {
            foreach (var button in GetComponentsInChildren<Button>())
            {
                button.interactable = CreatureSelector.value != 0;
            }
        }

        void PopulateDropdowns()
        {
            var allCreatures = FindObjectsOfType<CreatureController>();
            var optionsData = new List<Dropdown.OptionData>();

            foreach (var creature in allCreatures)
            {
                connectedCreatures.Add(creature);
                optionsData.Add(new Dropdown.OptionData { text = creature.name });
            }

            CreatureSelector.AddOptions(optionsData);
        }

        void RefreshButtons()
        {
            foreach (var button in currentButtons)
            {
                Destroy(button); // Destroy all current buttons.
            }
            currentButtons.Clear();

            foreach (var creature in connectedCreatures)
            {
                creature.SetAudioVolume(0.0f);
            }

            if (CreatureSelector.value > 0)
            {
                var activeMenu = connectedCreatures[CreatureSelector.value - 1];
                activeMenu.SetAudioVolume(1.0f);

                foreach (var animationTrigger in activeMenu.GetCreatureAnimationTriggers())
                {
                    var button = Instantiate(templateButton, animationContent);

                    button.GetComponentInChildren<Text>().text = animationTrigger;
                    button.SetActive(true);

                    button.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        activeMenu.Play(animationTrigger);
                    });

                    currentButtons.Add(button);
                }
            }

            ToggleButtons();
        }
    }
}
