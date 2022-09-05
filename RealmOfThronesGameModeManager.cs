

// ROT.RealmOfThronesGameModeManager
using System;
using System.Collections.Generic;
using ROT;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem.Load;

public class RealmOfThronesGameModeManager : MBGameManager
{
	private bool _loadingSavedGame;

	private LoadResult _loadedGameResult;

	private int _seed = 1234;

	public override UnitSpawnPrioritizations UnitSpawnPrioritization { get; }

	public RealmOfThronesGameModeManager()
	{
		_loadingSavedGame = false;
		_seed = (int)DateTime.Now.Ticks & 0xFFFF;
	}

	public RealmOfThronesGameModeManager(int seed)
	{
		_loadingSavedGame = false;
		_seed = seed;
	}

	public RealmOfThronesGameModeManager(LoadResult loadedGameResult)
	{
		_loadingSavedGame = true;
		_loadedGameResult = loadedGameResult;
	}

	public override void OnGameEnd(Game game)
	{
		MBDebug.SetErrorReportScene(null);
		base.OnGameEnd(game);
	}

	protected override void DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingStep, out GameManagerLoadingSteps nextStep)
	{
		nextStep = GameManagerLoadingSteps.None;
		switch (gameManagerLoadingStep)
		{
		case GameManagerLoadingSteps.PreInitializeZerothStep:
			nextStep = GameManagerLoadingSteps.FirstInitializeFirstStep;
			break;
		case GameManagerLoadingSteps.FirstInitializeFirstStep:
			MBGameManager.LoadModuleData(_loadingSavedGame);
			nextStep = GameManagerLoadingSteps.WaitSecondStep;
			break;
		case GameManagerLoadingSteps.WaitSecondStep:
			if (!_loadingSavedGame)
			{
				MBGameManager.StartNewGame();
			}
			nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
			break;
		case GameManagerLoadingSteps.SecondInitializeThirdState:
			MBGlobals.InitializeReferences();
			if (!_loadingSavedGame)
			{
				MBDebug.Print("Initializing new game begin...");
				Campaign campaign = new Campaign(CampaignGameMode.Campaign);
				Game.CreateGame(campaign, this);
				campaign.SetLoadingParameters(Campaign.GameLoadingType.NewCampaign);
				MBDebug.Print("Initializing new game end...");
			}
			else
			{
				MBDebug.Print("Initializing saved game begin...");
				((Campaign)Game.LoadSaveGame(_loadedGameResult, this).GameType).SetLoadingParameters(Campaign.GameLoadingType.SavedCampaign);
				_loadedGameResult = null;
				Common.MemoryCleanupGC();
				MBDebug.Print("Initializing saved game end...");
			}
			Game.Current.DoLoading();
			nextStep = GameManagerLoadingSteps.PostInitializeFourthState;
			break;
		case GameManagerLoadingSteps.PostInitializeFourthState:
		{
			bool flag = true;
			foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
			{
				flag = flag && subModule.DoLoading(Game.Current);
			}
			nextStep = (flag ? GameManagerLoadingSteps.FinishLoadingFifthStep : GameManagerLoadingSteps.PostInitializeFourthState);
			break;
		}
		case GameManagerLoadingSteps.FinishLoadingFifthStep:
			nextStep = (Game.Current.DoLoading() ? GameManagerLoadingSteps.None : GameManagerLoadingSteps.FinishLoadingFifthStep);
			break;
		}
	}

	public override void OnLoadFinished()
	{
		if (!_loadingSavedGame)
		{
			MBDebug.Print("Switching to menu window...");
			if (!Game.Current.IsDevelopmentMode)
			{
				LaunchSandboxCharacterCreation();
			}
		}
		else
		{
			if (CampaignSiegeTestStatic.IsSiegeTestBuild)
			{
				CampaignSiegeTestStatic.DisableSiegeTest();
			}
			Game.Current.GameStateManager.OnSavedGameLoadFinished();
			Game.Current.GameStateManager.CleanAndPushState(Game.Current.GameStateManager.CreateState<MapState>());
			string text = null;
			if (Game.Current.GameStateManager.ActiveState is MapState)
			{
				text = ((MapState)Game.Current.GameStateManager.ActiveState).GameMenuId;
			}
			if (!string.IsNullOrEmpty(text))
			{
				PlayerEncounter.Current?.OnLoad();
				Campaign.Current.GameMenuManager.SetNextMenu(text);
			}
			PartyBase.MainParty.Visuals?.SetMapIconAsDirty();
			Campaign.Current.CampaignInformationManager.OnGameLoaded();
			foreach (Settlement item in Settlement.All)
			{
				item.Party.Visuals.RefreshLevelMask(item.Party);
			}
			CampaignEventDispatcher.Instance.OnGameLoadFinished();
			((MapState)Game.Current.GameStateManager.ActiveState).OnLoadingFinished();
		}
		base.IsLoaded = true;
	}

	private void LaunchSandboxCharacterCreation()
	{
		Game.Current.GameStateManager.CleanAndPushState(Game.Current.GameStateManager.CreateState<CharacterCreationState>(new object[1]
		{
			new ROTCharacterCreationContent()
		}));
	}

	[CrashInformationCollector.CrashInformationProvider]
	private static CrashInformationCollector.CrashInformation UsedModuleInfoCrashCallback()
	{
		if (Campaign.Current?.PreviouslyUsedModules == null)
		{
			return null;
		}
		string[] moduleNames = SandBoxManager.Instance.ModuleManager.ModuleNames;
		List<(string, string)> list = new List<(string, string)>();
		foreach (string previouslyUsedModule in Campaign.Current.PreviouslyUsedModules)
		{
			string module = previouslyUsedModule;
			bool flag = moduleNames.FindIndex((string x) => x == module) != -1;
			list.Add((module, flag ? "1" : "0"));
		}
		return new CrashInformationCollector.CrashInformation("Used Mods", list);
	}
}
