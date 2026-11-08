global using DebugMod.VersionInterop;
#if SILKSONG
global using GameSceneManager = CustomSceneManager;
#else
global using GameSceneManager = global::SceneManager;
#endif
