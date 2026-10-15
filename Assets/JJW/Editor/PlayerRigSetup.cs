using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Player = ScienceProject.PlayerSystem.Player;
using PlayerInput = ScienceProject.PlayerSystem.PlayerInput;
using PlayerMove = ScienceProject.PlayerSystem.PlayerMove;
using PlayerAnimation = ScienceProject.PlayerSystem.PlayerAnimation;

namespace ScienceProject.EditorTools
{
    // Editor 폴더의 코드는 게임 빌드에 포함되지 않는다.
    // 런타임 네 클래스와 달리, 이 도구는 에셋 생성/씬 연결/검증 자동화 전용이다.
    [InitializeOnLoad]
    public static class PlayerRigSetup
    {
        private const string AnimationFolder = "Assets/JJW/Animation";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string InputPath = "Assets/Settings/InputSystem_Actions.inputactions";
        private const string RequestPath = "Temp/JJWPlayerRig.request";
        private const string ReportPath = "Library/JJWPlayerRig-report.txt";
        private const string TestKey = "ScienceProject.PlayerRig.PendingVerification";
        private const string BatchKey = "ScienceProject.PlayerRig.BatchVerification";
        private static int verificationUpdates;

        static PlayerRigSetup()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.delayCall += TryRunPending;
            if (SessionState.GetBool(TestKey, false))
                EditorApplication.update += WaitForPlayMode;
        }

        private static void TryRunPending()
        {
            if (!File.Exists(RequestPath) || SessionState.GetBool(TestKey, false)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryRunPending;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string command = File.ReadAllText(RequestPath).Trim();
            File.Delete(RequestPath);
            try
            {
                if (command == "setup-and-verify") SetupAndVerify();
                else Apply();
            }
            catch (Exception exception)
            {
                File.WriteAllText(ReportPath, "FAIL\n" + exception);
                Debug.LogException(exception);
            }
        }

        [MenuItem("Tools/Science Project/Set Up Top View Player")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Play Mode를 종료한 뒤 설정해 주세요.");

            AnimationClip[] idle = BuildClips("idle", 2, 4f, "Idle");
            AnimationClip[] walk = BuildClips("walk", 4, 8f, "Walk");
            AnimatorController controller = BuildController(idle, walk);

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (actions == null) throw new InvalidOperationException("기존 Input Actions 에셋이 없습니다.");
            InputAction action = actions.FindAction("Player/Move", true);
            const string referencePath = "Assets/JJW/Code/Player/PlayerMoveAction.asset";
            var reference = AssetDatabase.LoadAssetAtPath<InputActionReference>(referencePath);
            if (reference == null)
            {
                reference = InputActionReference.Create(action);
                reference.name = "PlayerMoveAction";
                AssetDatabase.CreateAsset(reference, referencePath);
            }
            else reference.Set(action);
            EditorUtility.SetDirty(reference);

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            GameObject root = scene.GetRootGameObjects().Single(o => o.name == "Player");
            Transform visual = root.transform.Find("Visual");
            if (visual == null) throw new InvalidOperationException("Player/Visual을 찾을 수 없습니다.");

            // 디스크에 반영되기 전, 열린 씬의 변경 사항까지 복사본으로 보존한다.
            if (!File.Exists("Library/JJWPlayerBeforeSetup.unity"))
                EditorSceneManager.SaveScene(scene, "Library/JJWPlayerBeforeSetup.unity", true);

            Rigidbody2D body = GetOrAdd<Rigidbody2D>(root);
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (root.GetComponent<Collider2D>() == null)
            {
                var collider = root.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(0.55f, 0.5f);
                collider.offset = new Vector2(0f, -0.25f);
            }

            Animator animator = GetOrAdd<Animator>(visual.gameObject);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var renderer = GetOrAdd<SpriteRenderer>(visual.gameObject);
            renderer.sprite = LoadSprites("idle")[0];
            renderer.flipX = false;
            var animation = GetOrAdd<PlayerAnimation>(visual.gameObject);
            var input = GetOrAdd<PlayerInput>(root);
            var mover = GetOrAdd<PlayerMove>(root);
            var player = GetOrAdd<Player>(root);
            Assign(input, "moveAction", reference);
            Assign(player, "playerInput", input);
            Assign(player, "playerMove", mover);
            Assign(player, "playerAnimation", animation);
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(ReportPath, "SETUP OK\nScene: " + ScenePath + "\nController: " + AssetDatabase.GetAssetPath(controller));
            Debug.Log("[PlayerRig] 탑뷰 이동 / Idle·Walk 블렌드 트리 / 씬 연결 완료.");
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite[] LoadSprites(string name)
        {
            return AssetDatabase.LoadAllAssetsAtPath("Assets/JJW/PlayerAssets/" + name + ".png")
                .OfType<Sprite>().OrderBy(s => int.Parse(s.name.Substring(name.Length + 1))).ToArray();
        }

        private static AnimationClip[] BuildClips(string sheet, int frameCount, float fps, string mode)
        {
            Sprite[] sprites = LoadSprites(sheet);
            if (sprites.Length != frameCount * 3)
                throw new InvalidOperationException(sheet + " 스프라이트 개수가 예상과 다릅니다.");
            string[] directions = { "Down", "Right", "Up" };
            var clips = new AnimationClip[3];
            for (int row = 0; row < 3; row++)
            {
                string path = AnimationFolder + "/Player_" + directions[row] + "_" + mode + ".anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, path);
                }
                clip.name = "Player_" + directions[row] + "_" + mode;
                clip.frameRate = fps;
                var frames = new ObjectReferenceKeyframe[frameCount + 1];
                for (int frame = 0; frame <= frameCount; frame++)
                    frames[frame] = new ObjectReferenceKeyframe
                    {
                        time = frame / fps,
                        value = sprites[row * frameCount + frame % frameCount]
                    };
                var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
                AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                settings.startTime = 0f;
                settings.stopTime = frameCount / fps;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
                clips[row] = clip;
            }
            return clips;
        }

        private static AnimatorController BuildController(AnimationClip[] idle, AnimationClip[] walk)
        {
            string path = AnimationFolder + "/Visual.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.parameters = Array.Empty<AnimatorControllerParameter>();
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            AnimatorControllerParameter[] parameters = controller.parameters;
            parameters.Single(p => p.name == "MoveY").defaultFloat = -1f;
            controller.parameters = parameters;
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState old in machine.states.ToArray())
                if (old.state.name != "Idle" && old.state.name != "Walk") machine.RemoveState(old.state);
            AnimatorState idleState = MakeState(controller, machine, "Idle", idle, new Vector3(250, 70));
            AnimatorState walkState = MakeState(controller, machine, "Walk", walk, new Vector3(520, 70));
            idleState.transitions = Array.Empty<AnimatorStateTransition>();
            walkState.transitions = Array.Empty<AnimatorStateTransition>();
            AddTransition(idleState, walkState, AnimatorConditionMode.If);
            AddTransition(walkState, idleState, AnimatorConditionMode.IfNot);
            machine.defaultState = idleState;
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(machine);
            return controller;
        }

        private static AnimatorState MakeState(AnimatorController controller, AnimatorStateMachine machine,
            string name, AnimationClip[] clips, Vector3 position)
        {
            AnimatorState state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name)
                ?? machine.AddState(name, position);
            var tree = state.motion as BlendTree;
            if (tree == null)
            {
                tree = new BlendTree();
                AssetDatabase.AddObjectToAsset(tree, controller);
            }
            tree.name = name + " Direction";
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveY";
            tree.useAutomaticThresholds = false;
            tree.children = new[]
            {
                Child(clips[0], Vector2.down), Child(clips[1], Vector2.right),
                Child(clips[2], Vector2.up), Child(clips[1], Vector2.left)
            };
            state.motion = tree;
            state.speed = 1f;
            state.writeDefaultValues = false;
            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(tree);
            return state;
        }

        private static ChildMotion Child(Motion clip, Vector2 position)
        {
            return new ChildMotion { motion = clip, position = position, timeScale = 1f, directBlendParameter = "" };
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode condition)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(condition, 0f, "IsMoving");
        }

        [MenuItem("Tools/Science Project/Verify Top View Player")]
        public static void SetupAndVerify()
        {
            Apply();
            SessionState.SetBool(TestKey, true);
            verificationUpdates = 0;
            EditorApplication.update -= WaitForPlayMode;
            EditorApplication.update += WaitForPlayMode;
            EditorApplication.EnterPlaymode();
        }

        // Unity 커맨드라인의 -executeMethod 진입점. 이미 열린 에디터에서는 메뉴를 쓴다.
        public static void BatchSetupAndVerify()
        {
            SessionState.SetBool(BatchKey, true);
            SetupAndVerify();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(TestKey, false))
                verificationUpdates = 0;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(BatchKey, false))
                {
                    SessionState.SetBool(BatchKey, false);
                    EditorApplication.Exit(File.ReadAllText(ReportPath).StartsWith("PASS") ? 0 : 1);
                }
                else EditorApplication.delayCall += TryRunPending;
            }
        }

        private static void WaitForPlayMode()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            // Awake/OnEnable 및 첫 입력 업데이트가 완료된 뒤 검증한다.
            if (++verificationUpdates < 8) return;
            EditorApplication.update -= WaitForPlayMode;
            VerifyInPlayMode();
        }

        private static void VerifyInPlayMode()
        {
            SessionState.SetBool(TestKey, false);
            Keyboard keyboard = null;
            GameObject wall = null;
            SimulationMode2D previousSimulationMode = Physics2D.simulationMode;
            try
            {
                // OS 이벤트/에디터 포커스는 제외하고 액션 바인딩부터 실제 물리까지 검증한다.
                var player = UnityEngine.Object.FindAnyObjectByType<Player>();
                Check(player != null && player.enabled, "Player coordinator exists");
                var input = player.GetComponent<PlayerInput>();
                var mover = player.GetComponent<PlayerMove>();
                var body = player.GetComponent<Rigidbody2D>();
                var animation = player.GetComponentInChildren<PlayerAnimation>();
                var animator = animation.GetComponent<Animator>();
                var renderer = animation.GetComponent<SpriteRenderer>();
                Check(body.gravityScale == 0f && body.freezeRotation, "top-view physics settings");
                Check(player.GetComponent<Collider2D>() != null, "player has a collider");
                keyboard = InputSystem.AddDevice<Keyboard>();
                animator.Rebind();
                animator.Update(0f);

                Key[] directions = { Key.S, Key.D, Key.W, Key.A };
                Vector2[] expected = { Vector2.down, Vector2.right, Vector2.up, Vector2.left };
                for (int i = 0; i < directions.Length; i++)
                {
                    SetKeyboard(keyboard, directions[i]);
                    Check(Vector2.Distance(input.ReadDirection(), expected[i]) < .001f,
                        "keyboard direction " + directions[i] + ": read=" + input.ReadDirection() +
                        ", enabled=" + input.enabled + ", pressed=" + keyboard[directions[i]].isPressed);
                    player.SendMessage("Update");
                    player.SendMessage("FixedUpdate");
                    animator.Update(0f);
                    animator.Update(.14f);
                    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"), "Walk blend tree entered");
                    Check(Mathf.Approximately(animator.GetFloat("MoveX"), expected[i].x), "MoveX direction");
                    Check(Mathf.Approximately(animator.GetFloat("MoveY"), expected[i].y), "MoveY direction");
                    Check(renderer.flipX == (directions[i] == Key.A), "left-only sprite reflection");
                    Sprite before = renderer.sprite;
                    animator.Update(.14f);
                    Check(renderer.sprite != before, "walk sprite frames advance");
                    SetKeyboard(keyboard);
                    player.SendMessage("Update");
                    player.SendMessage("FixedUpdate");
                    animator.Update(0f);
                    animator.Update(.01f);
                    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), "Idle blend tree entered");
                    Check(body.linearVelocity.sqrMagnitude < .0001f, "key release stops movement");
                    Check(Mathf.Approximately(animator.GetFloat("MoveX"), expected[i].x) &&
                        Mathf.Approximately(animator.GetFloat("MoveY"), expected[i].y), "idle retains last facing");
                }

                SetKeyboard(keyboard, Key.W, Key.D);
                player.SendMessage("Update");
                player.SendMessage("FixedUpdate");
                float diagonalSpeed = body.linearVelocity.magnitude;
                SetKeyboard(keyboard, Key.D);
                player.SendMessage("Update");
                player.SendMessage("FixedUpdate");
                Check(Mathf.Abs(diagonalSpeed - body.linearVelocity.magnitude) < .001f, "diagonal speed equals axial speed");
                SetKeyboard(keyboard, Key.UpArrow);
                Check(input.ReadDirection().y > .99f, "arrow key binding");

                Physics2D.simulationMode = SimulationMode2D.Script;
                body.position = Vector2.zero;
                wall = new GameObject("Player verification wall");
                wall.transform.position = new Vector3(1.5f, 0f, 0f);
                wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 6f);
                Physics2D.SyncTransforms();
                for (int i = 0; i < 100; i++)
                {
                    mover.Move(Vector2.right);
                    Physics2D.Simulate(.02f);
                }
                Check(body.position.x < .8f && body.position.x > .4f, "wall blocks movement");
                player.enabled = false;
                Check(body.linearVelocity.sqrMagnitude < .0001f, "disabling coordinator stops body");
                input.enabled = false;
                input.enabled = true;
                SetKeyboard(keyboard, Key.W);
                Check(input.ReadDirection().y > .99f, "input survives disable/enable");
                string report = "PASS\nUnity " + Application.unityVersion +
                    "\nWASD / arrow keys / four facing directions / diagonal normalization" +
                    "\nIdle and Walk blend trees / animated sprite frames / last-facing idle / flipX" +
                    "\nRigidbody2D wall collision / disable stop / input re-enable";
                File.WriteAllText(ReportPath, report);
                Debug.Log("[PlayerRig] " + report);
            }
            catch (Exception exception)
            {
                File.WriteAllText(ReportPath, "FAIL\n" + exception);
                Debug.LogException(exception);
            }
            finally
            {
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
                if (wall != null) UnityEngine.Object.Destroy(wall);
                Physics2D.simulationMode = previousSimulationMode;
                EditorApplication.ExitPlaymode();
            }
        }

        private static void SetKeyboard(Keyboard keyboard, params Key[] keys)
        {
            // 배치 에디터에서는 Game View 입력 프레임이 없으므로 테스트에서만
            // Input System의 플레이어 버퍼를 준비하고 공식 InputState.Change로 주입한다.
            // 실제 게임의 네 런타임 클래스에서는 리플렉션을 사용하지 않는다.
            object manager = typeof(InputSystem).GetField("s_Manager",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            manager.GetType().GetMethod("OnBeforeUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager,
                new object[] { InputUpdateType.Dynamic });
            InputState.Change(keyboard, new KeyboardState(keys), InputUpdateType.Dynamic);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Verification failed: " + message);
        }
    }
}
