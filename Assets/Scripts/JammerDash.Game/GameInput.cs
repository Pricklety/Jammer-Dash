//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.8.2
//     from Assets/InputSystem_Actions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine;

namespace JammerDash.Game
{
    public partial class @GameInput: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @GameInput()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputSystem_Actions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""3e852f20-078a-45f7-b1c6-68a952ecf5ce"",
            ""actions"": [
                {
                    ""name"": ""up"",
                    ""type"": ""Button"",
                    ""id"": ""e8e05e08-5481-479e-8f04-994f6af79a96"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""down"",
                    ""type"": ""Button"",
                    ""id"": ""c82903e3-ace3-4756-89ad-6ef1fe05f489"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ground"",
                    ""type"": ""Button"",
                    ""id"": ""e706b0a0-0cfb-45a9-8189-77f273735030"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""boost"",
                    ""type"": ""Button"",
                    ""id"": ""2831614e-65d4-45a1-920e-db2793e3b566"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""key1"",
                    ""type"": ""Button"",
                    ""id"": ""d0eabcd7-eb92-4269-a130-80f515cef43e"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""key2"",
                    ""type"": ""Button"",
                    ""id"": ""60ffc1c6-1930-484d-a1ab-7c96da9ea873"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""dd181ec2-f515-400f-b1a4-cfa225a6681e"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""511f9509-7c21-4a3a-b4d6-58bad744ed5a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""74715799-bfa0-4bd1-97fd-9ba6be42139c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ground"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cea74a8e-3de3-4dc1-88df-c365db0d0624"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""boost"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8b271ea0-b7af-4ed0-8e7d-4ae15c07ea30"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""key1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d18502fc-a4da-40c2-a781-e31da1f01b0e"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""key1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""caa21f0c-18dd-4e7c-b58e-b48f911a59b4"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""key2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7a82783a-9053-4100-9093-a70319de32d4"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""key2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Touch"",
            ""bindingGroup"": ""Touch"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Joystick"",
            ""bindingGroup"": ""Joystick"",
            ""devices"": [
                {
                    ""devicePath"": ""<Joystick>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""XR"",
            ""bindingGroup"": ""XR"",
            ""devices"": [
                {
                    ""devicePath"": ""<XRController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
            // Player
            m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
            m_Player_up = m_Player.FindAction("up", throwIfNotFound: true);
            m_Player_down = m_Player.FindAction("down", throwIfNotFound: true);
            m_Player_ground = m_Player.FindAction("ground", throwIfNotFound: true);
            m_Player_boost = m_Player.FindAction("boost", throwIfNotFound: true);
            m_Player_key1 = m_Player.FindAction("key1", throwIfNotFound: true);
            m_Player_key2 = m_Player.FindAction("key2", throwIfNotFound: true);
        }

        ~@GameInput()
        {
            Debug.Assert(!m_Player.enabled, "This will cause a leak and performance issues, GameInput.Player.Disable() has not been called.");
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Player
        private readonly InputActionMap m_Player;
        private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
        private readonly InputAction m_Player_up;
        private readonly InputAction m_Player_down;
        private readonly InputAction m_Player_ground;
        private readonly InputAction m_Player_boost;
        private readonly InputAction m_Player_key1;
        private readonly InputAction m_Player_key2;
        public struct PlayerActions
        {
            private @GameInput m_Wrapper;
            public PlayerActions(@GameInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @up => m_Wrapper.m_Player_up;
            public InputAction @down => m_Wrapper.m_Player_down;
            public InputAction @ground => m_Wrapper.m_Player_ground;
            public InputAction @boost => m_Wrapper.m_Player_boost;
            public InputAction @key1 => m_Wrapper.m_Player_key1;
            public InputAction @key2 => m_Wrapper.m_Player_key2;
            public InputActionMap Get() { return m_Wrapper.m_Player; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
            public void AddCallbacks(IPlayerActions instance)
            {
                if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
                @up.started += instance.OnUp;
                @up.performed += instance.OnUp;
                @up.canceled += instance.OnUp;
                @down.started += instance.OnDown;
                @down.performed += instance.OnDown;
                @down.canceled += instance.OnDown;
                @ground.started += instance.OnGround;
                @ground.performed += instance.OnGround;
                @ground.canceled += instance.OnGround;
                @boost.started += instance.OnBoost;
                @boost.performed += instance.OnBoost;
                @boost.canceled += instance.OnBoost;
                @key1.started += instance.OnKey1;
                @key1.performed += instance.OnKey1;
                @key1.canceled += instance.OnKey1;
                @key2.started += instance.OnKey2;
                @key2.performed += instance.OnKey2;
                @key2.canceled += instance.OnKey2;
            }

            private void UnregisterCallbacks(IPlayerActions instance)
            {
                @up.started -= instance.OnUp;
                @up.performed -= instance.OnUp;
                @up.canceled -= instance.OnUp;
                @down.started -= instance.OnDown;
                @down.performed -= instance.OnDown;
                @down.canceled -= instance.OnDown;
                @ground.started -= instance.OnGround;
                @ground.performed -= instance.OnGround;
                @ground.canceled -= instance.OnGround;
                @boost.started -= instance.OnBoost;
                @boost.performed -= instance.OnBoost;
                @boost.canceled -= instance.OnBoost;
                @key1.started -= instance.OnKey1;
                @key1.performed -= instance.OnKey1;
                @key1.canceled -= instance.OnKey1;
                @key2.started -= instance.OnKey2;
                @key2.performed -= instance.OnKey2;
                @key2.canceled -= instance.OnKey2;
            }

            public void RemoveCallbacks(IPlayerActions instance)
            {
                if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IPlayerActions instance)
            {
                foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public PlayerActions @Player => new PlayerActions(this);
        private int m_KeyboardMouseSchemeIndex = -1;
        public InputControlScheme KeyboardMouseScheme
        {
            get
            {
                if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
                return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
            }
        }
        private int m_GamepadSchemeIndex = -1;
        public InputControlScheme GamepadScheme
        {
            get
            {
                if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
                return asset.controlSchemes[m_GamepadSchemeIndex];
            }
        }
        private int m_TouchSchemeIndex = -1;
        public InputControlScheme TouchScheme
        {
            get
            {
                if (m_TouchSchemeIndex == -1) m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
                return asset.controlSchemes[m_TouchSchemeIndex];
            }
        }
        private int m_JoystickSchemeIndex = -1;
        public InputControlScheme JoystickScheme
        {
            get
            {
                if (m_JoystickSchemeIndex == -1) m_JoystickSchemeIndex = asset.FindControlSchemeIndex("Joystick");
                return asset.controlSchemes[m_JoystickSchemeIndex];
            }
        }
        private int m_XRSchemeIndex = -1;
        public InputControlScheme XRScheme
        {
            get
            {
                if (m_XRSchemeIndex == -1) m_XRSchemeIndex = asset.FindControlSchemeIndex("XR");
                return asset.controlSchemes[m_XRSchemeIndex];
            }
        }
        public interface IPlayerActions
        {
            void OnUp(InputAction.CallbackContext context);
            void OnDown(InputAction.CallbackContext context);
            void OnGround(InputAction.CallbackContext context);
            void OnBoost(InputAction.CallbackContext context);
            void OnKey1(InputAction.CallbackContext context);
            void OnKey2(InputAction.CallbackContext context);
        }
    }
}
