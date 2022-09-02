using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ROT;

public class ROTCharacterCreationContent : CharacterCreationContentBase
{
    protected enum SandboxAgeOptions
    {
        YoungAdult = 20,
        Adult = 30,
        MiddleAged = 40,
        Elder = 50
    }

    protected enum OccupationTypes
    {
        Artisan,
        Bard,
        Retainer,
        Merchant,
        Farmer,
        Hunter,
        Vagabond,
        Mercenary,
        Herder,
        Healer,
        NumberOfTypes
    }

    protected const int FocusToAddYouthStart = 2;

    protected const int FocusToAddAdultStart = 4;

    protected const int FocusToAddMiddleAgedStart = 6;

    protected const int FocusToAddElderlyStart = 8;

    protected const int AttributeToAddYouthStart = 1;

    protected const int AttributeToAddAdultStart = 2;

    protected const int AttributeToAddMiddleAgedStart = 3;

    protected const int AttributeToAddElderlyStart = 4;

    protected readonly Dictionary<string, Vec2> StartingPoints = new()
    {
        {
            "freefolk",
            new Vec2(456f, 1135f)
        },
        {
            "nightswatch",
            new Vec2(494f, 1079f)
        },
        {
            "battania",
            new Vec2(360f, 868f)
        },
        {
            "bolton",
            new Vec2(557f, 923f)
        },
        {
            "sturgia",
            new Vec2(124f, 584f)
        },
        {
            "vale",
            new Vec2(580f, 655f)
        },
        {
            "river",
            new Vec2(383f, 535f)
        },
        {
            "vlandia",
            new Vec2(186f, 397f)
        },
        {
            "dragonstone",
            new Vec2(655f, 496f)
        },
        {
            "stormlands",
            new Vec2(567f, 248f)
        },
        {
            "reach",
            new Vec2(174f, 158f)
        },
        {
            "crownlands",
            new Vec2(510f, 400f)
        },
        {
            "aserai",
            new Vec2(688f, 128f)
        },
        {
            "empire",
            new Vec2(886f, 400f)
        },
        {
            "ghiscari",
            new Vec2(1520f, 165f)
        },
        {
            "khuzait",
            new Vec2(1406f, 460f)
        },
        {
            "sarnor",
            new Vec2(1311f, 500f)
        },
        {
            "norvos",
            new Vec2(1090f, 420f)
        },
        {
            "valyrian",
            new Vec2(1400f, 47f)
        },
        {
            "volantine",
            new Vec2(1250f, 120f)
        },
        {
            "qohorik",
            new Vec2(1300f, 420f)
        }
    };

    protected SandboxAgeOptions StartingAge = SandboxAgeOptions.YoungAdult;

    protected OccupationTypes FamilyOccupationType;

    protected TextObject EducationIntroductoryText = new("{=!}{EDUCATION_INTRO}");

    protected TextObject YouthIntroductoryText = new("{=!}{YOUTH_INTRO}");

    public override TextObject ReviewPageDescription => new("{=W6pKpEoT}You prepare to set off for a grand adventure in Alantia! Here is your character. Continue if you are ready, or go back to make changes.");

    public override IEnumerable<Type> CharacterCreationStages
    {
        get
        {
            yield return typeof(CharacterCreationCultureStage);
            yield return typeof(CharacterCreationFaceGeneratorStage);
            yield return typeof(CharacterCreationGenericStage);
            yield return typeof(CharacterCreationBannerEditorStage);
            yield return typeof(CharacterCreationClanNamingStage);
            yield return typeof(CharacterCreationReviewStage);
            yield return typeof(CharacterCreationOptionsStage);
        }
    }

    protected override void OnCultureSelected()
    {
        SelectedTitleType = 1;
        SelectedParentType = 0;
    }

    public override int GetSelectedParentType()
    {
        return SelectedParentType;
    }

    public override void OnCharacterCreationFinalized()
    {
        var culture = CharacterObject.PlayerCharacter.Culture;
        if (StartingPoints.TryGetValue(culture.StringId, out var value))
        {
            MobileParty.MainParty.Position2D = value;
        }
        else
        {
            MobileParty.MainParty.Position2D = Campaign.Current.DefaultStartingPosition;
            Debug.FailedAssert("Selected culture is not in the dictionary!", "C:\\Develop\\mb3\\Source\\Bannerlord\\TheKingdomsOfAlantia\\AlantiaCharacterCreationContent.cs");
        }

        if (GameStateManager.Current.ActiveState is MapState mapState)
        {
            mapState.Handler.ResetCamera(true, true);
            mapState.Handler.TeleportCameraToMainParty();
        }

        SetHeroAge((float)StartingAge);
    }

    protected override void OnInitialized(CharacterCreation characterCreation)
    {
        AddParentsMenu(characterCreation);
        AddChildhoodMenu(characterCreation);
        AddEducationMenu(characterCreation);
        AddYouthMenu(characterCreation);
        AddAdulthoodMenu(characterCreation);
        AddAgeSelectionMenu(characterCreation);
    }

    protected override void OnApplyCulture()
    {
    }

    protected void AddParentsMenu(CharacterCreation characterCreation)
    {
        var characterCreationMenu = new CharacterCreationMenu(new TextObject("{=b4lDDcli}Family"), new TextObject("{=XgFU1pCx}You were born into a family of..."), ParentsOnInit);
        var characterCreationCategory = characterCreationMenu.AddMenuCategory(FreefolkParentsOnCondition);
        characterCreationCategory.AddCategoryOption(new TextObject("{FreParO1}Freefolk Warriors"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption1OnConsequence, FreeFolkParentsOption1OnApply, new TextObject("{FreParT1}"));
        characterCreationCategory.AddCategoryOption(new TextObject("{FreParO2}Frostfang Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption2OnConsequence, FreeFolkParentsOption2OnApply, new TextObject("{FreParT2}"));
        characterCreationCategory.AddCategoryOption(new TextObject("{FreParO3}Thenn Cannibals"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption3OnConsequence, FreeFolkParentsOption3OnApply, new TextObject("{FreParT3}"));
        characterCreationCategory.AddCategoryOption(new TextObject("{FreParO4}Haunted Forest Hunters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Trade
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption4OnConsequence, FreeFolkParentsOption4OnApply, new TextObject("{FreParT4}"));
        characterCreationCategory.AddCategoryOption(new TextObject("{FreParO1}Shamans"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption5OnConsequence, FreeFolkParentsOption5OnApply, new TextObject("{FreParT5}"));
        characterCreationCategory.AddCategoryOption(new TextObject("{FreParO6}Bards"), new List<SkillObject>
        {
            DefaultSkills.Charm,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption6OnConsequence, FreeFolkParentsOption6OnApply, new TextObject("{FreParT6}"));
        var characterCreationCategory2 = characterCreationMenu.AddMenuCategory(NightsWatchParentsOnCondition);
        characterCreationCategory2.AddCategoryOption(new TextObject("{NiPareO1}Castle Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption1OnConsequence, NightsWatchParentsOption1OnApply, new TextObject("{NiPareT1}"));
        characterCreationCategory2.AddCategoryOption(new TextObject("{NiPareO2}Tavern Owners"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption2OnConsequence, NightsWatchParentsOption2OnApply, new TextObject("{NiPareT2}"));
        characterCreationCategory2.AddCategoryOption(new TextObject("{NiPareO3}Maester's Apprentices"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption3OnConsequence, NightsWatchParentsOption3OnApply, new TextObject("{NiPareT3}"));
        characterCreationCategory2.AddCategoryOption(new TextObject("{NiPareO4}Brigands"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Crossbow
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption4OnConsequence, NightsWatchParentsOption4OnApply, new TextObject("{NiPareT4}"));
        characterCreationCategory2.AddCategoryOption(new TextObject("{NiPareO5}Freeriders"), new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption5OnConsequence, NightsWatchParentsOption5OnApply, new TextObject("{NiPareT5}"));
        characterCreationCategory2.AddCategoryOption(new TextObject("{NiPareO6}Foresters"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption6OnConsequence, NightsWatchParentsOption6OnApply, new TextObject("{NiPareT6}"));
        var characterCreationCategory3 = characterCreationMenu.AddMenuCategory(BattanianParentsOnCondition);
        characterCreationCategory3.AddCategoryOption(new TextObject("{BatParO1}Stark Men at Arms"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption1OnConsequence, BattanianParentsOption1OnApply, new TextObject("{BatParT1}"));
        characterCreationCategory3.AddCategoryOption(new TextObject("{BatParO2}Hunters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption2OnConsequence, BattanianParentsOption2OnApply, new TextObject("{BatParT2}"));
        characterCreationCategory3.AddCategoryOption(new TextObject("{BatParO3}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption3OnConsequence, BattanianParentsOption3OnApply, new TextObject("{BatParT3}"));
        characterCreationCategory3.AddCategoryOption(new TextObject("{BatParO4}Blacksmiths"), new List<SkillObject>
        {
            DefaultSkills.Crafting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption4OnConsequence, BattanianParentsOption4OnApply, new TextObject("{BatParT4}"));
        characterCreationCategory3.AddCategoryOption(new TextObject("{BatParO5}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Medicine
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption5OnConsequence, BattanianParentsOption5OnApply, new TextObject("{BatParT5}"));
        characterCreationCategory3.AddCategoryOption(new TextObject("{BatParO6}Outlaws"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption6OnConsequence, BattanianParentsOption6OnApply, new TextObject("{BatParT6}"));
        var characterCreationCategory4 = characterCreationMenu.AddMenuCategory(BoltonParentsOnCondition);
        characterCreationCategory4.AddCategoryOption(new TextObject("{BolParO1}Dreafort Gatemen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption1OnConsequence, BoltonParentsOption1OnApply, new TextObject("{BolParT1}"));
        characterCreationCategory4.AddCategoryOption(new TextObject("{BolParO2}Frey Crossbowmen"), new List<SkillObject>
        {
            DefaultSkills.Crossbow,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption2OnConsequence, BoltonParentsOption2OnApply, new TextObject("{BolParT2}"));
        characterCreationCategory4.AddCategoryOption(new TextObject("{BolParO3}Karstark Vanguard"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption3OnConsequence, BoltonParentsOption3OnApply, new TextObject("{BolParT3}"));
        characterCreationCategory4.AddCategoryOption(new TextObject("{BolParO4}Forest Bandits"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption4OnConsequence, BoltonParentsOption4OnApply, new TextObject("{BolParT4}"));
        characterCreationCategory4.AddCategoryOption(new TextObject("{BolParO5}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption5OnConsequence, BoltonParentsOption5OnApply, new TextObject("{BolParT5}"));
        characterCreationCategory4.AddCategoryOption(new TextObject("{BolParO6}Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption6OnConsequence, BoltonParentsOption6OnApply, new TextObject("{BolParT6}"));
        var characterCreationCategory5 = characterCreationMenu.AddMenuCategory(SturgiaParentsOnCondition);
        characterCreationCategory5.AddCategoryOption(new TextObject("{StuParO1}Greyjoy Footmen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption1OnConsequence, SturgiaParentsOption1OnApply, new TextObject("{StuParT1}"));
        characterCreationCategory5.AddCategoryOption(new TextObject("{StuParO2}Ironborn Reavers"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Roguery
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption2OnConsequence, SturgiaParentsOption2OnApply, new TextObject("{StuParT2}"));
        characterCreationCategory5.AddCategoryOption(new TextObject("{StuParO3}Fishermen"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption3OnConsequence, SturgiaParentsOption3OnApply, new TextObject("{StuParT3}"));
        characterCreationCategory5.AddCategoryOption(new TextObject("{StuParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption4OnConsequence, SturgiaParentsOption4OnApply, new TextObject("{StuParT4}"));
        characterCreationCategory5.AddCategoryOption(new TextObject("{StuParO5}Drowned Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption5OnConsequence, SturgiaParentsOption5OnApply, new TextObject("{StuParT5}"));
        characterCreationCategory5.AddCategoryOption(new TextObject("{StuParO6}Deckhands"), new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption6OnConsequence, SturgiaParentsOption6OnApply, new TextObject("{StuParT6}"));
        var characterCreationCategory6 = characterCreationMenu.AddMenuCategory(ValeParentsOnCondition);
        characterCreationCategory6.AddCategoryOption(new TextObject("{ValParO1}Arryn Castle Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption1OnConsequence, ValeParentsOption1OnApply, new TextObject("{ValParT1}"));
        characterCreationCategory6.AddCategoryOption(new TextObject("{ValParO2}Knights of the Vale"), new List<SkillObject>
        {
            DefaultSkills.Polearm,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption2OnConsequence, ValeParentsOption2OnApply, new TextObject("{ValParT2}"));
        characterCreationCategory6.AddCategoryOption(new TextObject("{ValParO3}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption3OnConsequence, ValeParentsOption3OnApply, new TextObject("{ValParT3}"));
        characterCreationCategory6.AddCategoryOption(new TextObject("{ValParO4}Mountains of the Moon Hunters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption4OnConsequence, ValeParentsOption4OnApply, new TextObject("{ValParT4}"));
        characterCreationCategory6.AddCategoryOption(new TextObject("{ValParO5}Brigands"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Throwing
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption5OnConsequence, ValeParentsOption5OnApply, new TextObject("{ValParT5}"));
        characterCreationCategory6.AddCategoryOption(new TextObject("{ValParO6}Stonemasons"), new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption6OnConsequence, ValeParentsOption6OnApply, new TextObject("{ValParT6}"));
        var characterCreationCategory7 = characterCreationMenu.AddMenuCategory(RiverlandsParentsOnCondition);
        characterCreationCategory7.AddCategoryOption(new TextObject("{RivParO1}Tully Men at Arms"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption1OnConsequence, RiverlandsParentsOption1OnApply, new TextObject("{RivParT1}"));
        characterCreationCategory7.AddCategoryOption(new TextObject("{RivParO2}Tavern Owners"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption2OnConsequence, RiverlandsParentsOption2OnApply, new TextObject("{RivParT2}"));
        characterCreationCategory7.AddCategoryOption(new TextObject("{RivParO3}River Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption3OnConsequence, RiverlandsParentsOption3OnApply, new TextObject("{RivParT3}"));
        characterCreationCategory7.AddCategoryOption(new TextObject("{RivParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Medicine
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption4OnConsequence, RiverlandsParentsOption4OnApply, new TextObject("{RivParT4}"));
        characterCreationCategory7.AddCategoryOption(new TextObject("{RivParO5}River Pirates"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Crossbow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption5OnConsequence, RiverlandsParentsOption5OnApply, new TextObject("{RivParT5}"));
        characterCreationCategory7.AddCategoryOption(new TextObject("{RivParO6}Mallister Knights"), new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption6OnConsequence, RiverlandsParentsOption6OnApply, new TextObject("{RivParT6}"));
        var characterCreationCategory8 = characterCreationMenu.AddMenuCategory(VlandiaParentsOnCondition);
        characterCreationCategory8.AddCategoryOption(new TextObject("{VlaParO1}Lannister Soldiers"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption1OnConsequence, VlandiaParentsOption1OnApply, new TextObject("{VlaParT1}"));
        characterCreationCategory8.AddCategoryOption(new TextObject("{VlaParO2}Miners"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption2OnConsequence, VlandiaParentsOption2OnApply, new TextObject("{VlaParT2}"));
        characterCreationCategory8.AddCategoryOption(new TextObject("{VlaParO3}Shop Clerks"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption3OnConsequence, VlandiaParentsOption3OnApply, new TextObject("{VlaParT3}"));
        characterCreationCategory8.AddCategoryOption(new TextObject("{VlaParO4}City Thugs"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption4OnConsequence, VlandiaParentsOption4OnApply, new TextObject("{VlaParT4}"));
        characterCreationCategory8.AddCategoryOption(new TextObject("{VlaParO5}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Medicine
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption5OnConsequence, VlandiaParentsOption5OnApply, new TextObject("{VlaParT5}"));
        characterCreationCategory8.AddCategoryOption(new TextObject("{VlaParO6}Hill Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption6OnConsequence, VlandiaParentsOption6OnApply, new TextObject("{VlaParT6}"));
        var characterCreationCategory9 = characterCreationMenu.AddMenuCategory(DragonstoneParentsOnCondition);
        characterCreationCategory9.AddCategoryOption(new TextObject("{DraParO1}Dragonstone Castle Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption1OnConsequence, DragonstoneParentsOption1OnApply, new TextObject("{DraParT1}"));
        characterCreationCategory9.AddCategoryOption(new TextObject("{DraParO2}Blackwater Fishermen"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption2OnConsequence, DragonstoneParentsOption2OnApply, new TextObject("{DraParT2}"));
        characterCreationCategory9.AddCategoryOption(new TextObject("{DraParO3}Red Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption3OnConsequence, DragonstoneParentsOption3OnApply, new TextObject("{DraParT3}"));
        characterCreationCategory9.AddCategoryOption(new TextObject("{DraParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption4OnConsequence, DragonstoneParentsOption4OnApply, new TextObject("{DraParT4}"));
        characterCreationCategory9.AddCategoryOption(new TextObject("{DraParO5}Bandit Sailors"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption5OnConsequence, DragonstoneParentsOption5OnApply, new TextObject("{DraParT5}"));
        characterCreationCategory9.AddCategoryOption(new TextObject("{DraParO6}Castle Wall Sharpshooters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Crossbow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption6OnConsequence, DragonstoneParentsOption6OnApply, new TextObject("{DraParT6}"));
        var characterCreationCategory10 = characterCreationMenu.AddMenuCategory(StormlandsParentsOnCondition);
        characterCreationCategory10.AddCategoryOption(new TextObject("{StoParO1}Baratheon Footmen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption1OnConsequence, StormlandsParentsOption1OnApply, new TextObject("{StoParT1}"));
        characterCreationCategory10.AddCategoryOption(new TextObject("{StoParO2}Blacksmiths"), new List<SkillObject>
        {
            DefaultSkills.Crafting,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption2OnConsequence, StormlandsParentsOption2OnApply, new TextObject("{StoParT2}"));
        characterCreationCategory10.AddCategoryOption(new TextObject("{StoParO3}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption3OnConsequence, StormlandsParentsOption3OnApply, new TextObject("{StoParT3}"));
        characterCreationCategory10.AddCategoryOption(new TextObject("{StoParO4}Maester's Apprentice"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption4OnConsequence, StormlandsParentsOption4OnApply, new TextObject("{StoParT4}"));
        characterCreationCategory10.AddCategoryOption(new TextObject("{StoParO5}Kingswood Brigands"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption5OnConsequence, StormlandsParentsOption5OnApply, new TextObject("{StoParT5}"));
        characterCreationCategory10.AddCategoryOption(new TextObject("{StoParO6}Mistwood Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption6OnConsequence, StormlandsParentsOption6OnApply, new TextObject("{StoParT6}"));
        var characterCreationCategory11 = characterCreationMenu.AddMenuCategory(ReachParentsOnCondition);
        characterCreationCategory11.AddCategoryOption(new TextObject("{ReaParO1}Tyrell House Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption1OnConsequence, ReachParentsOption1OnApply, new TextObject("{ReaParT1}"));
        characterCreationCategory11.AddCategoryOption(new TextObject("{ReaParO2}Farmers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption2OnConsequence, ReachParentsOption2OnApply, new TextObject("{ReaParT2}"));
        characterCreationCategory11.AddCategoryOption(new TextObject("{ReaParO3}Tarly Horsemen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption3OnConsequence, ReachParentsOption3OnApply, new TextObject("{ReaParT3}"));
        characterCreationCategory11.AddCategoryOption(new TextObject("{ReaParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption4OnConsequence, ReachParentsOption4OnApply, new TextObject("{ReaParT4}"));
        characterCreationCategory11.AddCategoryOption(new TextObject("{ReaParO5}Oldtown Acolyte"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption5OnConsequence, ReachParentsOption5OnApply, new TextObject("{ReaParT5}"));
        characterCreationCategory11.AddCategoryOption(new TextObject("{ReaParO6}Rogues"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption6OnConsequence, ReachParentsOption6OnApply, new TextObject("{ReaParT6}"));
        var characterCreationCategory12 = characterCreationMenu.AddMenuCategory(CrownlandsParentsOnCondition);
        characterCreationCategory12.AddCategoryOption(new TextObject("{CroParO1}Goldcloaks"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption1OnConsequence, CrownlandsParentsOption1OnApply, new TextObject("{CroParT1}"));
        characterCreationCategory12.AddCategoryOption(new TextObject("{CroParO2}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption2OnConsequence, CrownlandsParentsOption2OnApply, new TextObject("{CroParT2}"));
        characterCreationCategory12.AddCategoryOption(new TextObject("{CroParO3}Faith Militant Acolytes"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.OneHanded
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption3OnConsequence, CrownlandsParentsOption3OnApply, new TextObject("{CroParT3}"));
        characterCreationCategory12.AddCategoryOption(new TextObject("{CroParO4}Flea Bottom Thugs"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption4OnConsequence, CrownlandsParentsOption4OnApply, new TextObject("{CroParT4}"));
        characterCreationCategory12.AddCategoryOption(new TextObject("{CroParO5}Brothel Owners"), new List<SkillObject>
        {
            DefaultSkills.Charm,
            DefaultSkills.Trade
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption5OnConsequence, CrownlandsParentsOption5OnApply, new TextObject("{CroParT5}"));
        characterCreationCategory12.AddCategoryOption(new TextObject("{CroParO6}Stonemason"), new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption6OnConsequence, CrownlandsParentsOption6OnApply, new TextObject("{CroParT6}"));
        var characterCreationCategory13 = characterCreationMenu.AddMenuCategory(AseraiParentsOnCondition);
        characterCreationCategory13.AddCategoryOption(new TextObject("{AseParO1}Martell Palace Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption1OnConsequence, AseraiParentsOption1OnApply, new TextObject("{AseParT1}"));
        characterCreationCategory13.AddCategoryOption(new TextObject("{AseParO2}Caravan Masters"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption2OnConsequence, AseraiParentsOption2OnApply, new TextObject("{AseParT2}"));
        characterCreationCategory13.AddCategoryOption(new TextObject("{AseParO3}Desert Bandits"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption3OnConsequence, AseraiParentsOption3OnApply, new TextObject("{AseParT3}"));
        characterCreationCategory13.AddCategoryOption(new TextObject("{AseParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption4OnConsequence, AseraiParentsOption4OnApply, new TextObject("{AseParT4}"));
        characterCreationCategory13.AddCategoryOption(new TextObject("{AseParO5}Horse Farmers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption5OnConsequence, AseraiParentsOption5OnApply, new TextObject("{AseParT5}"));
        characterCreationCategory13.AddCategoryOption(new TextObject("{AseParO6}Maester's Apprentice"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption6OnConsequence, AseraiParentsOption6OnApply, new TextObject("{AseParT6}"));
        var characterCreationCategory14 = characterCreationMenu.AddMenuCategory(EmpireParentsOnCondition);
        characterCreationCategory14.AddCategoryOption(new TextObject("{EmpParO1}Essosi City Guard"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption1OnConsequence, EmpireParentsOption1OnApply, new TextObject("{EmpParT1}"));
        characterCreationCategory14.AddCategoryOption(new TextObject("{EmpParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption2OnConsequence, EmpireParentsOption2OnApply, new TextObject("{EmpParT2}"));
        characterCreationCategory14.AddCategoryOption(new TextObject("{EmpParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption3OnConsequence, EmpireParentsOption3OnApply, new TextObject("{EmpParT3}"));
        characterCreationCategory14.AddCategoryOption(new TextObject("{EmpParO4}Red Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption4OnConsequence, EmpireParentsOption4OnApply, new TextObject("{EmpParT4}"));
        characterCreationCategory14.AddCategoryOption(new TextObject("{EmpParO5}Summer Sea Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption5OnConsequence, EmpireParentsOption5OnApply, new TextObject("{EmpParT5}"));
        characterCreationCategory14.AddCategoryOption(new TextObject("{EmpParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption6OnConsequence, EmpireParentsOption6OnApply, new TextObject("{EmpParT6}"));
        var characterCreationCategory15 = characterCreationMenu.AddMenuCategory(GhiscariParentsOnCondition);
        characterCreationCategory15.AddCategoryOption(new TextObject("{GhiParO1}Pyramid Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption1OnConsequence, GhiscariParentsOption1OnApply, new TextObject("{GhiParT1}"));
        characterCreationCategory15.AddCategoryOption(new TextObject("{GhiParO2}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption2OnConsequence, GhiscariParentsOption2OnApply, new TextObject("{GhiParT2}"));
        characterCreationCategory15.AddCategoryOption(new TextObject("{GhiParO3}Sons of the Harpy"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption3OnConsequence, GhiscariParentsOption3OnApply, new TextObject("{GhiParT3}"));
        characterCreationCategory15.AddCategoryOption(new TextObject("{GhiParO4}Shop Owners"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption4OnConsequence, GhiscariParentsOption4OnApply, new TextObject("{GhiParT4}"));
        characterCreationCategory15.AddCategoryOption(new TextObject("{GhiParO5}Wise Masters"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption5OnConsequence, GhiscariParentsOption5OnApply, new TextObject("{GhiParT5}"));
        characterCreationCategory15.AddCategoryOption(new TextObject("{GhiParO6}Mercenary Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption6OnConsequence, GhiscariParentsOption6OnApply, new TextObject("{GhiParT6}"));
        var characterCreationCategory16 = characterCreationMenu.AddMenuCategory(KhuzaitParentsOnCondition);
        characterCreationCategory16.AddCategoryOption(new TextObject("{KhuParO1}Dothraki Warriors"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption1OnConsequence, KhuzaitParentsOption1OnApply, new TextObject("{KhuParT1}"));
        characterCreationCategory16.AddCategoryOption(new TextObject("{KhuParO2}Dothraki Bowmen"), new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption2OnConsequence, KhuzaitParentsOption2OnApply, new TextObject("{KhuParT2}"));
        characterCreationCategory16.AddCategoryOption(new TextObject("{KhuParO3}Slave"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption3OnConsequence, KhuzaitParentsOption3OnApply, new TextObject("{KhuParT3}"));
        characterCreationCategory16.AddCategoryOption(new TextObject("{KhuParO4}Village Herder"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption4OnConsequence, KhuzaitParentsOption4OnApply, new TextObject("{KhuParT4}"));
        characterCreationCategory16.AddCategoryOption(new TextObject("{KhuParO5}Steppe Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption5OnConsequence, KhuzaitParentsOption5OnApply, new TextObject("{KhuParT5}"));
        characterCreationCategory16.AddCategoryOption(new TextObject("{KhuParO6}Village Warlocks"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption6OnConsequence, KhuzaitParentsOption6OnApply, new TextObject("{KhuParT6}"));
        var characterCreationCategory17 = characterCreationMenu.AddMenuCategory(SarnorParentsOnCondition);
        characterCreationCategory17.AddCategoryOption(new TextObject("{SarParO1}Saath City Guard"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SarnorParentsOption1OnConsequence, SarnorParentsOption1OnApply, new TextObject("{SarParT1}"));
        characterCreationCategory17.AddCategoryOption(new TextObject("{SarParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SarnorParentsOption2OnConsequence, SarnorParentsOption2OnApply, new TextObject("{SarParT2}"));
        characterCreationCategory17.AddCategoryOption(new TextObject("{SarParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SarnorParentsOption3OnConsequence, SarnorParentsOption3OnApply, new TextObject("{SarParT3}"));
        characterCreationCategory17.AddCategoryOption(new TextObject("{SarParO4}Ankasi Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SarnorParentsOption4OnConsequence, SarnorParentsOption4OnApply, new TextObject("{SarParT4}"));
        characterCreationCategory17.AddCategoryOption(new TextObject("{SarParO5}Shivering Sea Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SarnorParentsOption5OnConsequence, SarnorParentsOption5OnApply, new TextObject("{SarParT5}"));
        characterCreationCategory17.AddCategoryOption(new TextObject("{SarParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SarnorParentsOption6OnConsequence, SarnorParentsOption6OnApply, new TextObject("{SarParT6}"));
        var characterCreationCategory18 = characterCreationMenu.AddMenuCategory(NorvosParentsOnCondition);
        characterCreationCategory18.AddCategoryOption(new TextObject("{NorParO1}Norvos City Guard"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NorvosParentsOption1OnConsequence, NorvosParentsOption1OnApply, new TextObject("{NorParT1}"));
        characterCreationCategory18.AddCategoryOption(new TextObject("{NorParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NorvosParentsOption2OnConsequence, NorvosParentsOption2OnApply, new TextObject("{NorParT2}"));
        characterCreationCategory18.AddCategoryOption(new TextObject("{NorParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NorvosParentsOption3OnConsequence, NorvosParentsOption3OnApply, new TextObject("{NorParT3}"));
        characterCreationCategory18.AddCategoryOption(new TextObject("{NorParO4}Bearded Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NorvosParentsOption4OnConsequence, NorvosParentsOption4OnApply, new TextObject("{NorParT4}"));
        characterCreationCategory18.AddCategoryOption(new TextObject("{NorParO5}Noyne Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NorvosParentsOption5OnConsequence, NorvosParentsOption5OnApply, new TextObject("{NorParT5}"));
        characterCreationCategory18.AddCategoryOption(new TextObject("{NorParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NorvosParentsOption6OnConsequence, NorvosParentsOption6OnApply, new TextObject("{NorParT6}"));
        var characterCreationCategory19 = characterCreationMenu.AddMenuCategory(ValyrianParentsOnCondition);
        characterCreationCategory19.AddCategoryOption(new TextObject("{ValyParO1}Valyrian Soldiers"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValyrianParentsOption1OnConsequence, ValyrianParentsOption1OnApply, new TextObject("{ValyParT1}"));
        characterCreationCategory19.AddCategoryOption(new TextObject("{ValyParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValyrianParentsOption2OnConsequence, ValyrianParentsOption2OnApply, new TextObject("{ValyParT2}"));
        characterCreationCategory19.AddCategoryOption(new TextObject("{ValyParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValyrianParentsOption3OnConsequence, ValyrianParentsOption3OnApply, new TextObject("{ValyParT3}"));
        characterCreationCategory19.AddCategoryOption(new TextObject("{ValyParO4}Old Faith Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValyrianParentsOption4OnConsequence, ValyrianParentsOption4OnApply, new TextObject("{ValyParT4}"));
        characterCreationCategory19.AddCategoryOption(new TextObject("{ValyParO5}Sea of Sorrows Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValyrianParentsOption5OnConsequence, ValyrianParentsOption5OnApply, new TextObject("{ValyParT5}"));
        characterCreationCategory19.AddCategoryOption(new TextObject("{ValyParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValyrianParentsOption6OnConsequence, ValyrianParentsOption6OnApply, new TextObject("{ValyParT6}"));
        var characterCreationCategory20 = characterCreationMenu.AddMenuCategory(VolantineParentsOnCondition);
        characterCreationCategory20.AddCategoryOption(new TextObject("{VolParO1}Tigercloaks"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VolantineParentsOption1OnConsequence, VolantineParentsOption1OnApply, new TextObject("{VolParT1}"));
        characterCreationCategory20.AddCategoryOption(new TextObject("{VolParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VolantineParentsOption2OnConsequence, VolantineParentsOption2OnApply, new TextObject("{VolParT2}"));
        characterCreationCategory20.AddCategoryOption(new TextObject("{VolParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VolantineParentsOption3OnConsequence, VolantineParentsOption3OnApply, new TextObject("{VolParT3}"));
        characterCreationCategory20.AddCategoryOption(new TextObject("{VolParO4}Red Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VolantineParentsOption4OnConsequence, VolantineParentsOption4OnApply, new TextObject("{VolParT4}"));
        characterCreationCategory20.AddCategoryOption(new TextObject("{VolParO5}Rhoyne Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VolantineParentsOption5OnConsequence, VolantineParentsOption5OnApply, new TextObject("{VolParT5}"));
        characterCreationCategory20.AddCategoryOption(new TextObject("{VolParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VolantineParentsOption6OnConsequence, VolantineParentsOption6OnApply, new TextObject("{VolParT6}"));
        var characterCreationCategory21 = characterCreationMenu.AddMenuCategory(QohorikParentsOnCondition);
        characterCreationCategory21.AddCategoryOption(new TextObject("{QohParO1}Black Goat Soldiers"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, QohorikParentsOption1OnConsequence, QohorikParentsOption1OnApply, new TextObject("{QohParT1}"));
        characterCreationCategory21.AddCategoryOption(new TextObject("{QohParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, QohorikParentsOption2OnConsequence, QohorikParentsOption2OnApply, new TextObject("{QohParT2}"));
        characterCreationCategory21.AddCategoryOption(new TextObject("{QohParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, QohorikParentsOption3OnConsequence, QohorikParentsOption3OnApply, new TextObject("{QohParT3}"));
        characterCreationCategory21.AddCategoryOption(new TextObject("{QohParO4}Black Goat Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, QohorikParentsOption4OnConsequence, QohorikParentsOption4OnApply, new TextObject("{QohParT4}"));
        characterCreationCategory21.AddCategoryOption(new TextObject("{QohParO5}Qhoyne Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, QohorikParentsOption5OnConsequence, QohorikParentsOption5OnApply, new TextObject("{QohParT5}"));
        characterCreationCategory21.AddCategoryOption(new TextObject("{QohParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, QohorikParentsOption6OnConsequence, QohorikParentsOption6OnApply, new TextObject("{QohParT6}"));
        characterCreation.AddNewMenu(characterCreationMenu);
    }

    protected void AddChildhoodMenu(CharacterCreation characterCreation)
    {
        var characterCreationMenu = new CharacterCreationMenu(new TextObject("{=8Yiwt1z6}Early Childhood"), new TextObject("{=character_creation_content_16}As a child you were noted for..."), ChildhoodOnInit);
        characterCreation.AddNewMenu(characterCreationMenu);
        var characterCreationCategory = characterCreationMenu.AddMenuCategory();
        var text = new TextObject("{=kmM68Qx4}your leadership skills.");
        var list = new List<SkillObject>();
        list.Add(DefaultSkills.Leadership);
        list.Add(DefaultSkills.Tactics);
        var cunning = DefaultCharacterAttributes.Cunning;
        var focusToAdd = FocusToAdd;
        var skillLevelToAdd = SkillLevelToAdd;
        var attributeLevelToAdd = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect = ChildhoodYourLeadershipSkillsOnConsequence;
        CharacterCreationApplyFinalEffects onApply = ChildhoodGoodLeadingOnApply;
        var descriptionText = new TextObject("{=FfNwXtii}If the wolf pup gang of your early childhood had an alpha, it was definitely you. All the other kids followed your lead as you decided what to play and where to play, and led them in games and mischief.");
        characterCreationCategory.AddCategoryOption(text, list, cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd, null, onSelect, onApply, descriptionText);
        var text2 = new TextObject("{=5HXS8HEY}your brawn.");
        var list2 = new List<SkillObject>();
        list2.Add(DefaultSkills.TwoHanded);
        list2.Add(DefaultSkills.Throwing);
        var vigor = DefaultCharacterAttributes.Vigor;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect2 = ChildhoodYourBrawnOnConsequence;
        CharacterCreationApplyFinalEffects onApply2 = ChildhoodGoodAthleticsOnApply;
        var descriptionText2 = new TextObject("{=YKzuGc54}You were big, and other children looked to have you around in any scrap with children from a neighboring village. You pushed a plough and throw an axe like an adult.");
        characterCreationCategory.AddCategoryOption(text2, list2, vigor, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, null, onSelect2, onApply2, descriptionText2);
        var text3 = new TextObject("{=QrYjPUEf}your attention to detail.");
        var list3 = new List<SkillObject>();
        list3.Add(DefaultSkills.Athletics);
        list3.Add(DefaultSkills.Bow);
        var control = DefaultCharacterAttributes.Control;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect3 = ChildhoodAttentionToDetailOnConsequence;
        CharacterCreationApplyFinalEffects onApply3 = ChildhoodGoodMemoryOnApply;
        var descriptionText3 = new TextObject("{=JUSHAPnu}You were quick on your feet and attentive to what was going on around you. Usually you could run away from trouble, though you could give a good account of yourself in a fight with other children if cornered.");
        characterCreationCategory.AddCategoryOption(text3, list3, control, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, null, onSelect3, onApply3, descriptionText3);
        var text4 = new TextObject("{=Y3UcaX74}your aptitude for numbers.");
        var list4 = new List<SkillObject>();
        list4.Add(DefaultSkills.Engineering);
        list4.Add(DefaultSkills.Trade);
        var intelligence = DefaultCharacterAttributes.Intelligence;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect4 = ChildhoodAptitudeForNumbersOnConsequence;
        CharacterCreationApplyFinalEffects onApply4 = ChildhoodGoodMathOnApply;
        var descriptionText4 = new TextObject("{=DFidSjIf}Most children around you had only the most rudimentary education, but you lingered after class to study letters and mathematics. You were fascinated by the marketplace - weights and measures, tallies and accounts, the chatter about profits and losses.");
        characterCreationCategory.AddCategoryOption(text4, list4, intelligence, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, null, onSelect4, onApply4, descriptionText4);
        var text5 = new TextObject("{=GEYzLuwb}your way with people.");
        var list5 = new List<SkillObject>();
        list5.Add(DefaultSkills.Charm);
        list5.Add(DefaultSkills.Leadership);
        var social = DefaultCharacterAttributes.Social;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect5 = ChildhoodWayWithPeopleOnConsequence;
        CharacterCreationApplyFinalEffects onApply5 = ChildhoodGoodMannersOnApply;
        var descriptionText5 = new TextObject("{=w2TEQq26}You were always attentive to other people, good at guessing their motivations. You studied how individuals were swayed, and tried out what you learned from adults on your friends.");
        characterCreationCategory.AddCategoryOption(text5, list5, social, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, null, onSelect5, onApply5, descriptionText5);
        var text6 = new TextObject("{=MEgLE2kj}your skill with horses.");
        var list6 = new List<SkillObject>();
        list6.Add(DefaultSkills.Riding);
        list6.Add(DefaultSkills.Medicine);
        var endurance = DefaultCharacterAttributes.Endurance;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect6 = ChildhoodSkillsWithHorsesOnConsequence;
        CharacterCreationApplyFinalEffects onApply6 = ChildhoodAffinityWithAnimalsOnApply;
        var descriptionText6 = new TextObject("{=ngazFofr}You were always drawn to animals, and spent as much time as possible hanging out in the village stables. You could calm horses, and were sometimes called upon to break in new colts. You learned the basics of veterinary arts, much of which is applicable to humans as well.");
        characterCreationCategory.AddCategoryOption(text6, list6, endurance, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, null, onSelect6, onApply6, descriptionText6);
        characterCreation.AddNewMenu(characterCreationMenu);
    }

    protected static void ChildhoodYourLeadershipSkillsOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_leader" });
    }

    protected static void ChildhoodYourBrawnOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
    }

    protected static void ChildhoodAttentionToDetailOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_memory" });
    }

    protected static void ChildhoodAptitudeForNumbersOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_numbers" });
    }

    protected static void ChildhoodWayWithPeopleOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
    }

    protected static void ChildhoodSkillsWithHorsesOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_animals" });
    }

    protected static void ChildhoodGoodLeadingOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void ChildhoodGoodAthleticsOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void ChildhoodGoodMemoryOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void ChildhoodGoodMathOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void ChildhoodGoodMannersOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void ChildhoodAffinityWithAnimalsOnApply(CharacterCreation characterCreation)
    {
    }

    protected void AddEducationMenu(CharacterCreation characterCreation)
    {
        var characterCreationMenu = new CharacterCreationMenu(new TextObject("{=rcoueCmk}Adolescence"), EducationIntroductoryText, EducationOnInit);
        characterCreation.AddNewMenu(characterCreationMenu);
        var characterCreationCategory = characterCreationMenu.AddMenuCategory();
        var text = new TextObject("{=RKVNvimC}herded the sheep.");
        var list = new List<SkillObject>();
        list.Add(DefaultSkills.Athletics);
        list.Add(DefaultSkills.Throwing);
        var control = DefaultCharacterAttributes.Control;
        var focusToAdd = FocusToAdd;
        var skillLevelToAdd = SkillLevelToAdd;
        var attributeLevelToAdd = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect = RuralAdolescenceHerderOnConsequence;
        CharacterCreationApplyFinalEffects onApply = RuralAdolescenceHerderOnApply;
        var descriptionText = new TextObject("{=KfaqPpbK}You went with other fleet-footed youths to take the villages' sheep, goats or cattle to graze in pastures near the village. You were in charge of chasing down stray beasts, and always kept a big stone on hand to be hurled at lurking predators if necessary.");
        characterCreationCategory.AddCategoryOption(text, list, control, focusToAdd, skillLevelToAdd, attributeLevelToAdd, optionCondition, onSelect, onApply, descriptionText);
        var text2 = new TextObject("{=bTKiN0hr}worked in the village smithy.");
        var list2 = new List<SkillObject>();
        list2.Add(DefaultSkills.TwoHanded);
        list2.Add(DefaultSkills.Crafting);
        var vigor = DefaultCharacterAttributes.Vigor;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition2 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect2 = RuralAdolescenceSmithyOnConsequence;
        CharacterCreationApplyFinalEffects onApply2 = RuralAdolescenceSmithyOnApply;
        var descriptionText2 = new TextObject("{=y6j1bJTH}You were apprenticed to the local smith. You learned how to heat and forge metal, hammering for hours at a time until your muscles ached.");
        characterCreationCategory.AddCategoryOption(text2, list2, vigor, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, optionCondition2, onSelect2, onApply2, descriptionText2);
        var text3 = new TextObject("{=tI8ZLtoA}repaired projects.");
        var list3 = new List<SkillObject>();
        list3.Add(DefaultSkills.Crafting);
        list3.Add(DefaultSkills.Engineering);
        var intelligence = DefaultCharacterAttributes.Intelligence;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition3 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect3 = RuralAdolescenceRepairmanOnConsequence;
        CharacterCreationApplyFinalEffects onApply3 = RuralAdolescenceRepairmanOnApply;
        var descriptionText3 = new TextObject("{=6LFj919J}You helped dig wells, rethatch houses, and fix broken plows. You learned about the basics of construction, as well as what it takes to keep a farming community prosperous.");
        characterCreationCategory.AddCategoryOption(text3, list3, intelligence, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, optionCondition3, onSelect3, onApply3, descriptionText3);
        var text4 = new TextObject("{=TRwgSLD2}gathered herbs in the wild.");
        var list4 = new List<SkillObject>();
        list4.Add(DefaultSkills.Medicine);
        list4.Add(DefaultSkills.Scouting);
        var endurance = DefaultCharacterAttributes.Endurance;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition4 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect4 = RuralAdolescenceGathererOnConsequence;
        CharacterCreationApplyFinalEffects onApply4 = RuralAdolescenceGathererOnApply;
        var descriptionText4 = new TextObject("{=9ks4u5cH}You were sent by the village healer up into the hills to look for useful medicinal plants. You learned which herbs healed wounds or brought down a fever, and how to find them.");
        characterCreationCategory.AddCategoryOption(text4, list4, endurance, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, optionCondition4, onSelect4, onApply4, descriptionText4);
        var text5 = new TextObject("{=T7m7ReTq}hunted small game.");
        var list5 = new List<SkillObject>();
        list5.Add(DefaultSkills.Bow);
        list5.Add(DefaultSkills.Tactics);
        var control2 = DefaultCharacterAttributes.Control;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition5 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect5 = RuralAdolescenceHunterOnConsequence;
        CharacterCreationApplyFinalEffects onApply5 = RuralAdolescenceHunterOnApply;
        var descriptionText5 = new TextObject("{=RuvSk3QT}You accompanied a local hunter as he went into the wilderness, helping him set up traps and catch small animals.");
        characterCreationCategory.AddCategoryOption(text5, list5, control2, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, optionCondition5, onSelect5, onApply5, descriptionText5);
        var text6 = new TextObject("{=qAbMagWq}sold produce at the market.");
        var list6 = new List<SkillObject>();
        list6.Add(DefaultSkills.Trade);
        list6.Add(DefaultSkills.Charm);
        var social = DefaultCharacterAttributes.Social;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition6 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect6 = RuralAdolescenceHelperOnConsequence;
        CharacterCreationApplyFinalEffects onApply6 = RuralAdolescenceHelperOnApply;
        var descriptionText6 = new TextObject("{=DIgsfYfz}You took your family's goods to the nearest town to sell your produce and buy supplies. It was hard work, but you enjoyed the hubbub of the marketplace.");
        characterCreationCategory.AddCategoryOption(text6, list6, social, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, optionCondition6, onSelect6, onApply6, descriptionText6);
        var text7 = new TextObject("{=nOfSqRnI}at the town watch's training ground.");
        var list7 = new List<SkillObject>();
        list7.Add(DefaultSkills.Crossbow);
        list7.Add(DefaultSkills.Tactics);
        var control3 = DefaultCharacterAttributes.Control;
        var focusToAdd7 = FocusToAdd;
        var skillLevelToAdd7 = SkillLevelToAdd;
        var attributeLevelToAdd7 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition7 = UrbanAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect7 = UrbanAdolescenceWatcherOnConsequence;
        CharacterCreationApplyFinalEffects onApply7 = UrbanAdolescenceWatcherOnApply;
        var descriptionText7 = new TextObject("{=qnqdEJOv}You watched the town's watch practice shooting and perfect their plans to defend the walls in case of a siege.");
        characterCreationCategory.AddCategoryOption(text7, list7, control3, focusToAdd7, skillLevelToAdd7, attributeLevelToAdd7, optionCondition7, onSelect7, onApply7, descriptionText7);
        var text8 = new TextObject("{=8a6dnLd2}with the alley gangs.");
        var list8 = new List<SkillObject>();
        list8.Add(DefaultSkills.Roguery);
        list8.Add(DefaultSkills.OneHanded);
        var cunning = DefaultCharacterAttributes.Cunning;
        var focusToAdd8 = FocusToAdd;
        var skillLevelToAdd8 = SkillLevelToAdd;
        var attributeLevelToAdd8 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition8 = UrbanAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect8 = UrbanAdolescenceGangerOnConsequence;
        CharacterCreationApplyFinalEffects onApply8 = UrbanAdolescenceGangerOnApply;
        var descriptionText8 = new TextObject("{=1SUTcF0J}The gang leaders who kept watch over the slums of cities were always in need of poor youth to run messages and back them up in turf wars, while thrill-seeking merchants' sons and daughters sometimes slummed it in their company as well.");
        characterCreationCategory.AddCategoryOption(text8, list8, cunning, focusToAdd8, skillLevelToAdd8, attributeLevelToAdd8, optionCondition8, onSelect8, onApply8, descriptionText8);
        var text9 = new TextObject("{=7Hv984Sf}at docks and building sites.");
        var list9 = new List<SkillObject>();
        list9.Add(DefaultSkills.Athletics);
        list9.Add(DefaultSkills.Crafting);
        var vigor2 = DefaultCharacterAttributes.Vigor;
        var focusToAdd9 = FocusToAdd;
        var skillLevelToAdd9 = SkillLevelToAdd;
        var attributeLevelToAdd9 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition9 = UrbanAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect9 = UrbanAdolescenceDockerOnConsequence;
        CharacterCreationApplyFinalEffects onApply9 = UrbanAdolescenceDockerOnApply;
        var descriptionText9 = new TextObject("{=bhdkegZ4}All towns had their share of projects that were constantly in need of both skilled and unskilled labor. You learned how hoists and scaffolds were constructed, how planks and stones were hewn and fitted, and other skills.");
        characterCreationCategory.AddCategoryOption(text9, list9, vigor2, focusToAdd9, skillLevelToAdd9, attributeLevelToAdd9, optionCondition9, onSelect9, onApply9, descriptionText9);
        var text10 = new TextObject("{=kbcwb5TH}in the markets and caravanserais.");
        var list10 = new List<SkillObject>();
        list10.Add(DefaultSkills.Trade);
        list10.Add(DefaultSkills.Charm);
        var social2 = DefaultCharacterAttributes.Social;
        var focusToAdd10 = FocusToAdd;
        var skillLevelToAdd10 = SkillLevelToAdd;
        var attributeLevelToAdd10 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition10 = UrbanPoorAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect10 = UrbanAdolescenceMarketerOnConsequence;
        CharacterCreationApplyFinalEffects onApply10 = UrbanAdolescenceMarketerOnApply;
        var descriptionText10 = new TextObject("{=lLJh7WAT}You worked in the marketplace, selling trinkets and drinks to busy shoppers.");
        characterCreationCategory.AddCategoryOption(text10, list10, social2, focusToAdd10, skillLevelToAdd10, attributeLevelToAdd10, optionCondition10, onSelect10, onApply10, descriptionText10);
        var text11 = new TextObject("{=kbcwb5TH}in the markets and caravanserais.");
        var list11 = new List<SkillObject>();
        list11.Add(DefaultSkills.Trade);
        list11.Add(DefaultSkills.Charm);
        var social3 = DefaultCharacterAttributes.Social;
        var focusToAdd11 = FocusToAdd;
        var skillLevelToAdd11 = SkillLevelToAdd;
        var attributeLevelToAdd11 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition11 = UrbanRichAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect11 = UrbanAdolescenceMarketerOnConsequence;
        CharacterCreationApplyFinalEffects onApply11 = UrbanAdolescenceMarketerOnApply;
        var descriptionText11 = new TextObject("{=rmMcwSn8}You helped your family handle their business affairs, going down to the marketplace to make purchases and oversee the arrival of caravans.");
        characterCreationCategory.AddCategoryOption(text11, list11, social3, focusToAdd11, skillLevelToAdd11, attributeLevelToAdd11, optionCondition11, onSelect11, onApply11, descriptionText11);
        var text12 = new TextObject("{=mfRbx5KE}reading and studying.");
        var list12 = new List<SkillObject>();
        list12.Add(DefaultSkills.Engineering);
        list12.Add(DefaultSkills.Leadership);
        var intelligence2 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd12 = FocusToAdd;
        var skillLevelToAdd12 = SkillLevelToAdd;
        var attributeLevelToAdd12 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition12 = UrbanPoorAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect12 = UrbanAdolescenceTutorOnConsequence;
        CharacterCreationApplyFinalEffects onApply12 = UrbanAdolescenceDockerOnApply;
        var descriptionText12 = new TextObject("{=elQnygal}Your family scraped up the money for a rudimentary schooling and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates.");
        characterCreationCategory.AddCategoryOption(text12, list12, intelligence2, focusToAdd12, skillLevelToAdd12, attributeLevelToAdd12, optionCondition12, onSelect12, onApply12, descriptionText12);
        var text13 = new TextObject("{=etG87fB7}with your tutor.");
        var list13 = new List<SkillObject>();
        list13.Add(DefaultSkills.Engineering);
        list13.Add(DefaultSkills.Leadership);
        var intelligence3 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd13 = FocusToAdd;
        var skillLevelToAdd13 = SkillLevelToAdd;
        var attributeLevelToAdd13 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition13 = UrbanRichAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect13 = UrbanAdolescenceTutorOnConsequence;
        CharacterCreationApplyFinalEffects onApply13 = UrbanAdolescenceDockerOnApply;
        var descriptionText13 = new TextObject("{=hXl25avg}Your family arranged for a private tutor and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates.");
        characterCreationCategory.AddCategoryOption(text13, list13, intelligence3, focusToAdd13, skillLevelToAdd13, attributeLevelToAdd13, optionCondition13, onSelect13, onApply13, descriptionText13);
        var text14 = new TextObject("{=FKpLEamz}caring for horses.");
        var list14 = new List<SkillObject>();
        list14.Add(DefaultSkills.Riding);
        list14.Add(DefaultSkills.Steward);
        var endurance2 = DefaultCharacterAttributes.Endurance;
        var focusToAdd14 = FocusToAdd;
        var skillLevelToAdd14 = SkillLevelToAdd;
        var attributeLevelToAdd14 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition14 = UrbanRichAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect14 = UrbanAdolescenceHorserOnConsequence;
        CharacterCreationApplyFinalEffects onApply14 = UrbanAdolescenceDockerOnApply;
        var descriptionText14 = new TextObject("{=Ghz90npw}Your family owned a few horses at the town stables and you took charge of their care. Many evenings you would take them out beyond the walls and gallup through the fields, racing other youth.");
        characterCreationCategory.AddCategoryOption(text14, list14, endurance2, focusToAdd14, skillLevelToAdd14, attributeLevelToAdd14, optionCondition14, onSelect14, onApply14, descriptionText14);
        var text15 = new TextObject("{=vH7GtuuK}working at the stables.");
        var list15 = new List<SkillObject>();
        list15.Add(DefaultSkills.Riding);
        list15.Add(DefaultSkills.Steward);
        var endurance3 = DefaultCharacterAttributes.Endurance;
        var focusToAdd15 = FocusToAdd;
        var skillLevelToAdd15 = SkillLevelToAdd;
        var attributeLevelToAdd15 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition15 = UrbanPoorAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect15 = UrbanAdolescenceHorserOnConsequence;
        CharacterCreationApplyFinalEffects onApply15 = UrbanAdolescenceDockerOnApply;
        var descriptionText15 = new TextObject("{=csUq1RCC}You were employed as a hired hand at the town's stables. The overseers recognized that you had a knack for horses, and you were allowed to exercise them and sometimes even break in new steeds.");
        characterCreationCategory.AddCategoryOption(text15, list15, endurance3, focusToAdd15, skillLevelToAdd15, attributeLevelToAdd15, optionCondition15, onSelect15, onApply15, descriptionText15);
        characterCreation.AddNewMenu(characterCreationMenu);
    }

    protected void RuralAdolescenceHerderOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_streets" });
        RefreshPropsAndClothing(characterCreation, false, "carry_bostaff_rogue1", true);
    }

    protected void RuralAdolescenceSmithyOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_militia" });
        RefreshPropsAndClothing(characterCreation, false, "peasant_hammer_1_t1", true);
    }

    protected void RuralAdolescenceRepairmanOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_grit" });
        RefreshPropsAndClothing(characterCreation, false, "carry_hammer", true);
    }

    protected void RuralAdolescenceGathererOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers" });
        RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_basket_a", true);
    }

    protected void RuralAdolescenceHunterOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
        RefreshPropsAndClothing(characterCreation, false, "composite_bow", true);
    }

    protected void RuralAdolescenceHelperOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers_2" });
        RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_fabric_c", true);
    }

    protected void UrbanAdolescenceWatcherOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_fox" });
        RefreshPropsAndClothing(characterCreation, false, "", true);
    }

    protected void UrbanAdolescenceMarketerOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
        RefreshPropsAndClothing(characterCreation, false, "", true);
    }

    protected void UrbanAdolescenceGangerOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
        RefreshPropsAndClothing(characterCreation, false, "", true);
    }

    protected void UrbanAdolescenceDockerOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers" });
        RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_basket_a", true);
    }

    protected void UrbanAdolescenceHorserOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers_2" });
        RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_fabric_c", true);
    }

    protected void UrbanAdolescenceTutorOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_book" });
        RefreshPropsAndClothing(characterCreation, false, "character_creation_notebook", false);
    }

    protected static void RuralAdolescenceHerderOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void RuralAdolescenceSmithyOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void RuralAdolescenceRepairmanOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void RuralAdolescenceGathererOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void RuralAdolescenceHunterOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void RuralAdolescenceHelperOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void UrbanAdolescenceWatcherOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void UrbanAdolescenceMarketerOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void UrbanAdolescenceGangerOnApply(CharacterCreation characterCreation)
    {
    }

    protected static void UrbanAdolescenceDockerOnApply(CharacterCreation characterCreation)
    {
    }

    protected void AddYouthMenu(CharacterCreation characterCreation)
    {
        var characterCreationMenu = new CharacterCreationMenu(new TextObject("{=ok8lSW6M}Youth"), YouthIntroductoryText, YouthOnInit);
        var characterCreationCategory = characterCreationMenu.AddMenuCategory();
        var text = new TextObject("{=TraWitAr}Trained with the Army");
        var list = new List<SkillObject>();
        list.Add(DefaultSkills.OneHanded);
        list.Add(DefaultSkills.Bow);
        var vigor = DefaultCharacterAttributes.Vigor;
        var focusToAdd = FocusToAdd;
        var skillLevelToAdd = SkillLevelToAdd;
        var attributeLevelToAdd = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition = YouthCommanderOnCondition;
        CharacterCreationOnSelect onSelect = YouthCommanderOnConsequence;
        CharacterCreationApplyFinalEffects onApply = YouthCommanderOnApply;
        var descriptionText = new TextObject("{=TraArmTx}You spent your time at the training grounds of your home region learning the basics of weapon combat. Through this training you became proficient in Single Handed combat and Bowcraft");
        characterCreationCategory.AddCategoryOption(text, list, vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd, optionCondition, onSelect, onApply, descriptionText);
        var text2 = new TextObject("{=SpeTimMa}Spent time in the Markets");
        var list2 = new List<SkillObject>();
        list2.Add(DefaultSkills.Charm);
        list2.Add(DefaultSkills.Roguery);
        var social = DefaultCharacterAttributes.Social;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition2 = YouthMerchantOnCondition;
        CharacterCreationOnSelect onSelect2 = YouthMerchantOnConsequence;
        CharacterCreationApplyFinalEffects onApply2 = YouthMerchantOnApply;
        var descriptionText2 = new TextObject("{=TimMarTx}You spent your time in the various markets across Westeros. Here you learned the arts of trade, from charming your patrons to slyly backstabbing your competition.");
        characterCreationCategory.AddCategoryOption(text2, list2, social, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, optionCondition2, onSelect2, onApply2, descriptionText2);
        var text3 = new TextObject("{=TraAcWes}Traveled across Westeros");
        var list3 = new List<SkillObject>();
        list3.Add(DefaultSkills.Crossbow);
        list3.Add(DefaultSkills.Scouting);
        var cunning = DefaultCharacterAttributes.Cunning;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition3 = YouthWanderlustOnCondition;
        CharacterCreationOnSelect onSelect3 = YouthWanderlustOnConsequence;
        CharacterCreationApplyFinalEffects onApply3 = YouthWanderlustOnApply;
        var descriptionText3 = new TextObject("{=TraWesTx}You spent your time traveling across the lands Westeros. You learned how to navigate across treacherous terrain, armed with a crossbow to protect yourself you became proficient as a traveler.");
        characterCreationCategory.AddCategoryOption(text3, list3, cunning, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, optionCondition3, onSelect3, onApply3, descriptionText3);
        var text4 = new TextObject("{=HidWiBan}Hid out with Bandits");
        var list4 = new List<SkillObject>();
        list4.Add(DefaultSkills.Leadership);
        list4.Add(DefaultSkills.Polearm);
        var control = DefaultCharacterAttributes.Control;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition4 = YouthRaiderOnCondition;
        CharacterCreationOnSelect onSelect4 = YouthRaiderOnConsequence;
        CharacterCreationApplyFinalEffects onApply4 = YouthRaiderOnApply;
        var descriptionText4 = new TextObject("{=HidBanTx}You spent time with the bandits of your home, raiding and attacking the various villages nearby. You learned how to organise large groups and conduct yourself as a leader, they also showed you how to use polearms to protect yourself.");
        characterCreationCategory.AddCategoryOption(text4, list4, control, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, optionCondition4, onSelect4, onApply4, descriptionText4);
        var text5 = new TextObject("{=LeaToSmi}Learning how to Smith");
        var list5 = new List<SkillObject>();
        list5.Add(DefaultSkills.Crafting);
        list5.Add(DefaultSkills.Athletics);
        var endurance = DefaultCharacterAttributes.Endurance;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition5 = YouthSmithOnCondition;
        CharacterCreationOnSelect onSelect5 = YouthSmithOnConsequence;
        CharacterCreationApplyFinalEffects onApply5 = YouthSmithOnApply;
        var descriptionText5 = new TextObject("{=LeaSmiTx}Your time was spent working in the forges. Because of this you understand how metals work together and the making of various weapons. Your body has grown strong because this work so you can endure in a fight for much longer.");
        characterCreationCategory.AddCategoryOption(text5, list5, endurance, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, optionCondition5, onSelect5, onApply5, descriptionText5);
        var text6 = new TextObject("{=}Stood Guard with the Militia");
        var list6 = new List<SkillObject>();
        list6.Add(DefaultSkills.Bow);
        list6.Add(DefaultSkills.Engineering);
        var intelligence = DefaultCharacterAttributes.Intelligence;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition6 = YouthMilitiaOnCondition;
        CharacterCreationOnSelect onSelect6 = YouthMilitiaOnConsequence;
        CharacterCreationApplyFinalEffects onApply6 = YouthMilitiaOnApply;
        var descriptionText6 = new TextObject("{=}");
        characterCreationCategory.AddCategoryOption(text6, list6, intelligence, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, optionCondition6, onSelect6, onApply6, descriptionText6);
        characterCreation.AddNewMenu(characterCreationMenu);
    }

    protected void YouthCommanderOnApply(CharacterCreation characterCreation)
    {
    }

    protected void YouthMerchantOnApply(CharacterCreation characterCreation)
    {
    }

    protected void YouthWanderlustOnApply(CharacterCreation characterCreation)
    {
    }

    protected void YouthRaiderOnApply(CharacterCreation characterCreation)
    {
    }

    protected void YouthSmithOnApply(CharacterCreation characterCreation)
    {
    }

    protected void YouthMilitiaOnApply(CharacterCreation characterCreation)
    {
    }

    protected void YouthCommanderOnConsequence(CharacterCreation characterCreation)
    {
        SelectedTitleType = 1;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    protected void YouthMerchantOnConsequence(CharacterCreation characterCreation)
    {
        SelectedTitleType = 2;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    protected void YouthWanderlustOnConsequence(CharacterCreation characterCreation)
    {
        SelectedTitleType = 3;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    protected void YouthRaiderOnConsequence(CharacterCreation characterCreation)
    {
        SelectedTitleType = 4;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    protected void YouthSmithOnConsequence(CharacterCreation characterCreation)
    {
        SelectedTitleType = 5;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    protected void YouthMilitiaOnConsequence(CharacterCreation characterCreation)
    {
        SelectedTitleType = 6;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    protected bool YouthCommanderOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = !(stringId == "qohorik") ? 1 : 0;
                break;
            case "freefolk":
            case "bolton":
            case "nightswatch":
            case "vale":
            case "river":
            case "dragonstone":
            case "stormlands":
            case "reach":
            case "ghiscari":
            case "crownlands":
            case "empire":
            case "battania":
            case "aserai":
            case "vlandia":
            case "sturgia":
            case "khuzait":
            case "sarnor":
            case "norvos":
            case "valyrian":
            case "volantine":
                num = 0;
                break;
        }

        return num == 0;
    }

    protected bool YouthMerchantOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = !(stringId == "qohorik") ? 1 : 0;
                break;
            case "freefolk":
            case "bolton":
            case "nightswatch":
            case "vale":
            case "river":
            case "dragonstone":
            case "stormlands":
            case "reach":
            case "ghiscari":
            case "crownlands":
            case "empire":
            case "battania":
            case "aserai":
            case "vlandia":
            case "sturgia":
            case "khuzait":
            case "sarnor":
            case "norvos":
            case "valyrian":
            case "volantine":
                num = 0;
                break;
        }

        return num == 0;
    }

    protected bool YouthWanderlustOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = !(stringId == "qohorik") ? 1 : 0;
                break;
            case "freefolk":
            case "bolton":
            case "nightswatch":
            case "vale":
            case "river":
            case "dragonstone":
            case "stormlands":
            case "reach":
            case "ghiscari":
            case "crownlands":
            case "empire":
            case "battania":
            case "aserai":
            case "vlandia":
            case "sturgia":
            case "khuzait":
            case "sarnor":
            case "norvos":
            case "valyrian":
            case "volantine":
                num = 0;
                break;
        }

        return num == 0;
    }

    protected bool YouthRaiderOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = !(stringId == "qohorik") ? 1 : 0;
                break;
            case "freefolk":
            case "bolton":
            case "nightswatch":
            case "vale":
            case "river":
            case "dragonstone":
            case "stormlands":
            case "reach":
            case "ghiscari":
            case "crownlands":
            case "empire":
            case "battania":
            case "aserai":
            case "vlandia":
            case "sturgia":
            case "khuzait":
            case "sarnor":
            case "norvos":
            case "valyrian":
            case "volantine":
                num = 0;
                break;
        }

        return num == 0;
    }

    protected bool YouthSmithOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = !(stringId == "qohorik") ? 1 : 0;
                break;
            case "freefolk":
            case "bolton":
            case "nightswatch":
            case "vale":
            case "river":
            case "dragonstone":
            case "stormlands":
            case "reach":
            case "ghiscari":
            case "crownlands":
            case "empire":
            case "battania":
            case "aserai":
            case "vlandia":
            case "sturgia":
            case "khuzait":
            case "sarnor":
            case "norvos":
            case "valyrian":
            case "volantine":
                num = 0;
                break;
        }

        return num == 0;
    }

    protected bool YouthMilitiaOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = !(stringId == "qohorik") ? 1 : 0;
                break;
            case "freefolk":
            case "bolton":
            case "nightswatch":
            case "vale":
            case "river":
            case "dragonstone":
            case "stormlands":
            case "reach":
            case "ghiscari":
            case "crownlands":
            case "empire":
            case "battania":
            case "aserai":
            case "vlandia":
            case "sturgia":
            case "khuzait":
            case "sarnor":
            case "norvos":
            case "valyrian":
            case "volantine":
                num = 0;
                break;
        }

        return num == 0;
    }

    protected void AddAdulthoodMenu(CharacterCreation characterCreation)
    {
        MBTextManager.SetTextVariable("EXP_VALUE", SkillLevelToAdd);
        var characterCreationMenu = new CharacterCreationMenu(new TextObject("{=MafIe9yI}Young Adulthood"), new TextObject("{=4WYY0X59}Before you set out for a life of adventure, your biggest achievement was..."), AccomplishmentOnInit);
        characterCreation.AddNewMenu(characterCreationMenu);
        var characterCreationCategory = characterCreationMenu.AddMenuCategory();
        var text = new TextObject("{=8bwpVpgy}you defeated an enemy in battle.");
        var list = new List<SkillObject>();
        list.Add(DefaultSkills.OneHanded);
        list.Add(DefaultSkills.TwoHanded);
        var vigor = DefaultCharacterAttributes.Vigor;
        var focusToAdd = FocusToAdd;
        var skillLevelToAdd = SkillLevelToAdd;
        var attributeLevelToAdd = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect = AccomplishmentDefeatedEnemyOnConsequence;
        CharacterCreationApplyFinalEffects onApply = AccomplishmentDefeatedEnemyOnApply;
        var descriptionText = new TextObject("{=1IEroJKs}Not everyone who musters for the levy marches to war, and not everyone who goes on campaign sees action. You did both, and you also took down an enemy warrior in direct one-to-one combat, in the full view of your comrades.");
        characterCreationCategory.AddCategoryOption(text, list, vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd, null, onSelect, onApply, descriptionText, new List<TraitObject> { DefaultTraits.Valor }, 1, 20);
        var text2 = new TextObject("{=mP3uFbcq}you led a successful manhunt.");
        var list2 = new List<SkillObject>();
        list2.Add(DefaultSkills.Tactics);
        list2.Add(DefaultSkills.Leadership);
        var cunning = DefaultCharacterAttributes.Cunning;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition = AccomplishmentPosseOnConditions;
        CharacterCreationOnSelect onSelect2 = AccomplishmentExpeditionOnConsequence;
        CharacterCreationApplyFinalEffects onApply2 = AccomplishmentExpeditionOnApply;
        var descriptionText2 = new TextObject("{=4f5xwzX0}When your community needed to organize a posse to pursue horse thieves, you were the obvious choice. You hunted down the raiders, surrounded them and forced their surrender, and took back your stolen property.");
        characterCreationCategory.AddCategoryOption(text2, list2, cunning, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, optionCondition, onSelect2, onApply2, descriptionText2, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text3 = new TextObject("{=wfbtS71d}you led a caravan.");
        var list3 = new List<SkillObject>();
        list3.Add(DefaultSkills.Tactics);
        list3.Add(DefaultSkills.Leadership);
        var cunning2 = DefaultCharacterAttributes.Cunning;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition2 = AccomplishmentMerchantOnCondition;
        CharacterCreationOnSelect onSelect3 = AccomplishmentMerchantOnConsequence;
        CharacterCreationApplyFinalEffects onApply3 = AccomplishmentExpeditionOnApply;
        var descriptionText3 = new TextObject("{=joRHKCkm}Your family needed someone trustworthy to take a caravan to a neighboring town. You organized supplies, ensured a constant watch to keep away bandits, and brought it safely to its destination.");
        characterCreationCategory.AddCategoryOption(text3, list3, cunning2, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, optionCondition2, onSelect3, onApply3, descriptionText3, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text4 = new TextObject("{=x1HTX5hq}you saved your village from a flood.");
        var list4 = new List<SkillObject>();
        list4.Add(DefaultSkills.Tactics);
        list4.Add(DefaultSkills.Leadership);
        var cunning3 = DefaultCharacterAttributes.Cunning;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition3 = AccomplishmentSavedVillageOnCondition;
        CharacterCreationOnSelect onSelect4 = AccomplishmentSavedVillageOnConsequence;
        CharacterCreationApplyFinalEffects onApply4 = AccomplishmentExpeditionOnApply;
        var descriptionText4 = new TextObject("{=bWlmGDf3}When a sudden storm caused the local stream to rise suddenly, your neighbors needed quick-thinking leadership. You provided it, directing them to build levees to save their homes.");
        characterCreationCategory.AddCategoryOption(text4, list4, cunning3, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, optionCondition3, onSelect4, onApply4, descriptionText4, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text5 = new TextObject("{=s8PNllPN}you saved your city quarter from a fire.");
        var list5 = new List<SkillObject>();
        list5.Add(DefaultSkills.Tactics);
        list5.Add(DefaultSkills.Leadership);
        var cunning4 = DefaultCharacterAttributes.Cunning;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition4 = AccomplishmentSavedStreetOnCondition;
        CharacterCreationOnSelect onSelect5 = AccomplishmentSavedStreetOnConsequence;
        CharacterCreationApplyFinalEffects onApply5 = AccomplishmentExpeditionOnApply;
        var descriptionText5 = new TextObject("{=ZAGR6PYc}When a sudden blaze broke out in a back alley, your neighbors needed quick-thinking leadership and you provided it. You organized a bucket line to the nearest well, putting the fire out before any homes were lost.");
        characterCreationCategory.AddCategoryOption(text5, list5, cunning4, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, optionCondition4, onSelect5, onApply5, descriptionText5, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text6 = new TextObject("{=xORjDTal}you invested some money in a workshop.");
        var list6 = new List<SkillObject>();
        list6.Add(DefaultSkills.Trade);
        list6.Add(DefaultSkills.Crafting);
        var intelligence = DefaultCharacterAttributes.Intelligence;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition5 = AccomplishmentUrbanOnCondition;
        CharacterCreationOnSelect onSelect6 = AccomplishmentWorkshopOnConsequence;
        CharacterCreationApplyFinalEffects onApply6 = AccomplishmentWorkshopOnApply;
        var descriptionText6 = new TextObject("{=PyVqDLBu}Your parents didn't give you much money, but they did leave just enough for you to secure a loan against a larger amount to build a small workshop. You paid back what you borrowed, and sold your enterprise for a profit.");
        characterCreationCategory.AddCategoryOption(text6, list6, intelligence, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, optionCondition5, onSelect6, onApply6, descriptionText6, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text7 = new TextObject("{=xKXcqRJI}you invested some money in land.");
        var list7 = new List<SkillObject>();
        list7.Add(DefaultSkills.Trade);
        list7.Add(DefaultSkills.Crafting);
        var intelligence2 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd7 = FocusToAdd;
        var skillLevelToAdd7 = SkillLevelToAdd;
        var attributeLevelToAdd7 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition6 = AccomplishmentRuralOnCondition;
        CharacterCreationOnSelect onSelect7 = AccomplishmentWorkshopOnConsequence;
        CharacterCreationApplyFinalEffects onApply7 = AccomplishmentWorkshopOnApply;
        var descriptionText7 = new TextObject("{=cbF9jdQo}Your parents didn't give you much money, but they did leave just enough for you to purchase a plot of unused land at the edge of the village. You cleared away rocks and dug an irrigation ditch, raised a few seasons of crops, than sold it for a considerable profit.");
        characterCreationCategory.AddCategoryOption(text7, list7, intelligence2, focusToAdd7, skillLevelToAdd7, attributeLevelToAdd7, optionCondition6, onSelect7, onApply7, descriptionText7, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text8 = new TextObject("{=TbNRtUjb}you hunted a dangerous animal.");
        var list8 = new List<SkillObject>();
        list8.Add(DefaultSkills.Polearm);
        list8.Add(DefaultSkills.Crossbow);
        var control = DefaultCharacterAttributes.Control;
        var focusToAdd8 = FocusToAdd;
        var skillLevelToAdd8 = SkillLevelToAdd;
        var attributeLevelToAdd8 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition7 = AccomplishmentRuralOnCondition;
        CharacterCreationOnSelect onSelect8 = AccomplishmentSiegeHunterOnConsequence;
        CharacterCreationApplyFinalEffects onApply8 = AccomplishmentSiegeHunterOnApply;
        var descriptionText8 = new TextObject("{=I3PcdaaL}Wolves, bears are a constant menace to the flocks of northern Calradia, while hyenas and leopards trouble the south. You went with a group of your fellow villagers and fired the missile that brought down the beast.");
        characterCreationCategory.AddCategoryOption(text8, list8, control, focusToAdd8, skillLevelToAdd8, attributeLevelToAdd8, optionCondition7, onSelect8, onApply8, descriptionText8, null, 0, 5);
        var text9 = new TextObject("{=WbHfGCbd}you survived a siege.");
        var list9 = new List<SkillObject>();
        list9.Add(DefaultSkills.Bow);
        list9.Add(DefaultSkills.Crossbow);
        var control2 = DefaultCharacterAttributes.Control;
        var focusToAdd9 = FocusToAdd;
        var skillLevelToAdd9 = SkillLevelToAdd;
        var attributeLevelToAdd9 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition8 = AccomplishmentUrbanOnCondition;
        CharacterCreationOnSelect onSelect9 = AccomplishmentSiegeHunterOnConsequence;
        CharacterCreationApplyFinalEffects onApply9 = AccomplishmentSiegeHunterOnApply;
        var descriptionText9 = new TextObject("{=FhZPjhli}Your hometown was briefly placed under siege, and you were called to defend the walls. Everyone did their part to repulse the enemy assault, and everyone is justly proud of what they endured.");
        characterCreationCategory.AddCategoryOption(text9, list9, control2, focusToAdd9, skillLevelToAdd9, attributeLevelToAdd9, optionCondition8, onSelect9, onApply9, descriptionText9, null, 0, 5);
        var text10 = new TextObject("{=kNXet6Um}you had a famous escapade in town.");
        var list10 = new List<SkillObject>();
        list10.Add(DefaultSkills.Athletics);
        list10.Add(DefaultSkills.Roguery);
        var endurance = DefaultCharacterAttributes.Endurance;
        var focusToAdd10 = FocusToAdd;
        var skillLevelToAdd10 = SkillLevelToAdd;
        var attributeLevelToAdd10 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition9 = AccomplishmentRuralOnCondition;
        CharacterCreationOnSelect onSelect10 = AccomplishmentEscapadeOnConsequence;
        CharacterCreationApplyFinalEffects onApply10 = AccomplishmentEscapadeOnApply;
        var descriptionText10 = new TextObject("{=DjeAJtix}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, on one of your trips into town you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.");
        characterCreationCategory.AddCategoryOption(text10, list10, endurance, focusToAdd10, skillLevelToAdd10, attributeLevelToAdd10, optionCondition9, onSelect10, onApply10, descriptionText10, new List<TraitObject> { DefaultTraits.Valor }, 1, 5);
        var text11 = new TextObject("{=qlOuiKXj}you had a famous escapade.");
        var list11 = new List<SkillObject>();
        list11.Add(DefaultSkills.Athletics);
        list11.Add(DefaultSkills.Roguery);
        var endurance2 = DefaultCharacterAttributes.Endurance;
        var focusToAdd11 = FocusToAdd;
        var skillLevelToAdd11 = SkillLevelToAdd;
        var attributeLevelToAdd11 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition10 = AccomplishmentUrbanOnCondition;
        CharacterCreationOnSelect onSelect11 = AccomplishmentEscapadeOnConsequence;
        CharacterCreationApplyFinalEffects onApply11 = AccomplishmentEscapadeOnApply;
        var descriptionText11 = new TextObject("{=lD5Ob3R4}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.");
        characterCreationCategory.AddCategoryOption(text11, list11, endurance2, focusToAdd11, skillLevelToAdd11, attributeLevelToAdd11, optionCondition10, onSelect11, onApply11, descriptionText11, new List<TraitObject> { DefaultTraits.Valor }, 1, 5);
        var text12 = new TextObject("{=Yqm0Dics}you treated people well.");
        var list12 = new List<SkillObject>();
        list12.Add(DefaultSkills.Charm);
        list12.Add(DefaultSkills.Steward);
        var social = DefaultCharacterAttributes.Social;
        var focusToAdd12 = FocusToAdd;
        var skillLevelToAdd12 = SkillLevelToAdd;
        var attributeLevelToAdd12 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect12 = AccomplishmentTreaterOnConsequence;
        CharacterCreationApplyFinalEffects onApply12 = AccomplishmentTreaterOnApply;
        var descriptionText12 = new TextObject("{=dDmcqTzb}Yours wasn't the kind of reputation that local legends are made of, but it was the kind that wins you respect among those around you. You were consistently fair and honest in your business dealings and helpful to those in trouble. In doing so, you got a sense of what made people tick.");
        characterCreationCategory.AddCategoryOption(text12, list12, social, focusToAdd12, skillLevelToAdd12, attributeLevelToAdd12, null, onSelect12, onApply12, descriptionText12, new List<TraitObject>
        {
            DefaultTraits.Mercy,
            DefaultTraits.Generosity,
            DefaultTraits.Honor
        }, 1, 5);
        characterCreation.AddNewMenu(characterCreationMenu);
    }

    protected void AccomplishmentDefeatedEnemyOnApply(CharacterCreation characterCreation)
    {
    }

    protected void AccomplishmentExpeditionOnApply(CharacterCreation characterCreation)
    {
    }

    protected bool AccomplishmentRuralOnCondition()
    {
        return RuralType();
    }

    protected bool AccomplishmentMerchantOnCondition()
    {
        return FamilyOccupationType == OccupationTypes.Merchant;
    }

    protected bool AccomplishmentPosseOnConditions()
    {
        return FamilyOccupationType == OccupationTypes.Retainer || FamilyOccupationType == OccupationTypes.Herder || FamilyOccupationType == OccupationTypes.Mercenary;
    }

    protected bool AccomplishmentSavedVillageOnCondition()
    {
        return RuralType() && FamilyOccupationType != OccupationTypes.Retainer && FamilyOccupationType != OccupationTypes.Herder;
    }

    protected bool AccomplishmentSavedStreetOnCondition()
    {
        return !RuralType() && FamilyOccupationType != OccupationTypes.Merchant && FamilyOccupationType != OccupationTypes.Mercenary;
    }

    protected bool AccomplishmentUrbanOnCondition()
    {
        return !RuralType();
    }

    protected void AccomplishmentWorkshopOnApply(CharacterCreation characterCreation)
    {
    }

    protected void AccomplishmentSiegeHunterOnApply(CharacterCreation characterCreation)
    {
    }

    protected void AccomplishmentEscapadeOnApply(CharacterCreation characterCreation)
    {
    }

    protected void AccomplishmentTreaterOnApply(CharacterCreation characterCreation)
    {
    }

    protected void AccomplishmentDefeatedEnemyOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
    }

    protected void AccomplishmentExpeditionOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_gracious" });
    }

    protected void AccomplishmentMerchantOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_ready" });
    }

    protected void AccomplishmentSavedVillageOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_vibrant" });
    }

    protected void AccomplishmentSavedStreetOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_vibrant" });
    }

    protected void AccomplishmentWorkshopOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_decisive" });
    }

    protected void AccomplishmentSiegeHunterOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_tough" });
    }

    protected void AccomplishmentEscapadeOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_clever" });
    }

    protected void AccomplishmentTreaterOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
    }

    protected void AddAgeSelectionMenu(CharacterCreation characterCreation)
    {
        MBTextManager.SetTextVariable("EXP_VALUE", SkillLevelToAdd);
        var characterCreationMenu = new CharacterCreationMenu(new TextObject("{=HDFEAYDk}Starting Age"), new TextObject("{=VlOGrGSn}Your character started off on the adventuring path at the age of..."), StartingAgeOnInit);
        var characterCreationCategory = characterCreationMenu.AddMenuCategory();
        characterCreationCategory.AddCategoryOption(new TextObject("{=!}20"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeYoungOnConsequence, StartingAgeYoungOnApply, new TextObject("{=2k7adlh7}While lacking experience a bit, you are full with youthful energy, you are fully eager, for the long years of adventuring ahead."), null, 0, 0, 0, 2, 1);
        characterCreationCategory.AddCategoryOption(new TextObject("{=!}30"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeAdultOnConsequence, StartingAgeAdultOnApply, new TextObject("{=NUlVFRtK}You are at your prime, You still have some youthful energy but also have a substantial amount of experience under your belt. "), null, 0, 0, 0, 4, 2);
        characterCreationCategory.AddCategoryOption(new TextObject("{=!}40"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeMiddleAgedOnConsequence, StartingAgeMiddleAgedOnApply, new TextObject("{=5MxTYApM}This is the right age for starting off, you have years of experience, and you are old enough for people to respect you and gather under your banner."), null, 0, 0, 0, 6, 3);
        characterCreationCategory.AddCategoryOption(new TextObject("{=!}50"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeElderlyOnConsequence, StartingAgeElderlyOnApply, new TextObject("{=ePD5Afvy}While you are past your prime, there is still enough time to go on that last big adventure for you. And you have all the experience you need to overcome anything!"), null, 0, 0, 0, 8, 4);
        characterCreation.AddNewMenu(characterCreationMenu);
    }

    protected void ParentsOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = false;
        characterCreation.HasSecondaryCharacter = false;
        ClearMountEntity(characterCreation);
        characterCreation.ClearFaceGenPrefab();
        if (PlayerBodyProperties != CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment))
        {
            PlayerBodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment);
            var motherBodyProperties = PlayerBodyProperties;
            var fatherBodyProperties = PlayerBodyProperties;
            FaceGen.GenerateParentKey(PlayerBodyProperties, CharacterObject.PlayerCharacter.Race, ref motherBodyProperties, ref fatherBodyProperties);
            motherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.3f, 0.2f), motherBodyProperties.StaticProperties);
            fatherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.5f, 0.5f), fatherBodyProperties.StaticProperties);
            MotherFacegenCharacter = new FaceGenChar(motherBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), true, "anim_mother_1");
            FatherFacegenCharacter = new FaceGenChar(fatherBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), false, "anim_father_1");
        }

        characterCreation.ChangeFaceGenChars(new List<FaceGenChar> { MotherFacegenCharacter, FatherFacegenCharacter });
        ChangeParentsOutfit(characterCreation);
        ChangeParentsAnimation(characterCreation);
    }

    protected void ChangeParentsOutfit(CharacterCreation characterCreation, string fatherItemId = "", string motherItemId = "", bool isLeftHandItemForFather = true, bool isLeftHandItemForMother = true)
    {
        characterCreation.ClearFaceGenPrefab();
        var list = new List<Equipment>();
        var equipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("mother_char_creation_" + SelectedParentType + "_" + GetSelectedCulture().StringId)?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
        var equipment2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("father_char_creation_" + SelectedParentType + "_" + GetSelectedCulture().StringId)?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
        if (motherItemId != "")
        {
            var @object = Game.Current.ObjectManager.GetObject<ItemObject>(motherItemId);
            if (@object != null)
            {
                equipment.AddEquipmentToSlotWithoutAgent(!isLeftHandItemForMother ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(@object));
            }
            else
            {
                var baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCreation.FaceGenChars[0].Race);
                characterCreation.ChangeCharacterPrefab(motherItemId, isLeftHandItemForMother ? baseMonsterFromRace.MainHandItemBoneIndex : baseMonsterFromRace.OffHandItemBoneIndex);
            }
        }

        if (fatherItemId != "")
        {
            var object2 = Game.Current.ObjectManager.GetObject<ItemObject>(fatherItemId);
            if (object2 != null) equipment2.AddEquipmentToSlotWithoutAgent(!isLeftHandItemForFather ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(object2));
        }

        list.Add(equipment);
        list.Add(equipment2);
        characterCreation.ChangeCharactersEquipment(list);
    }

    protected void ChangeParentsAnimation(CharacterCreation characterCreation)
    {
        var actionList = new List<string>
        {
            "anim_mother_" + SelectedParentType,
            "anim_father_" + SelectedParentType
        };
        characterCreation.ChangeCharsAnimation(actionList);
    }

    protected void SetParentAndOccupationType(CharacterCreation characterCreation, int parentType, OccupationTypes occupationType, string fatherItemId = "", string motherItemId = "", bool isLeftHandItemForFather = true, bool isLeftHandItemForMother = true)
    {
        SelectedParentType = parentType;
        FamilyOccupationType = occupationType;
        characterCreation.ChangeFaceGenChars(new List<FaceGenChar> { MotherFacegenCharacter, FatherFacegenCharacter });
        ChangeParentsAnimation(characterCreation);
        ChangeParentsOutfit(characterCreation, fatherItemId, motherItemId, isLeftHandItemForFather, isLeftHandItemForMother);
    }

    protected void FreeFolkParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void FreeFolkParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void FreeFolkParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void FreeFolkParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void FreeFolkParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void FreeFolkParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void NightsWatchParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void NightsWatchParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void NightsWatchParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void NightsWatchParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void NightsWatchParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void NightsWatchParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void BattanianParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void BattanianParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void BattanianParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void BattanianParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void BattanianParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void BattanianParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void BoltonParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void BoltonParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void BoltonParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void BoltonParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void BoltonParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void BoltonParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void SturgiaParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void SturgiaParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void SturgiaParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void SturgiaParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void SturgiaParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void SturgiaParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void ValeParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void ValeParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void ValeParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void ValeParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void ValeParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void ValeParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void RiverlandsParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void RiverlandsParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void RiverlandsParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void RiverlandsParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void RiverlandsParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void RiverlandsParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void VlandiaParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void VlandiaParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void VlandiaParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void VlandiaParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void VlandiaParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void VlandiaParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void DragonstoneParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void DragonstoneParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void DragonstoneParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void DragonstoneParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void DragonstoneParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void DragonstoneParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void StormlandsParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void StormlandsParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void StormlandsParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void StormlandsParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void StormlandsParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void StormlandsParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void ReachParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void ReachParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void ReachParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void ReachParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void ReachParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void ReachParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void CrownlandsParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void CrownlandsParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void CrownlandsParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void CrownlandsParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void CrownlandsParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void CrownlandsParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void AseraiParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void AseraiParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void AseraiParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void AseraiParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void AseraiParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void AseraiParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void EmpireParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void EmpireParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void EmpireParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void EmpireParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void EmpireParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void EmpireParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void GhiscariParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void GhiscariParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void GhiscariParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void GhiscariParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void GhiscariParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void GhiscariParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void KhuzaitParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void KhuzaitParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void KhuzaitParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void KhuzaitParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void KhuzaitParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void KhuzaitParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void SarnorParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void SarnorParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void SarnorParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void SarnorParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void SarnorParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void SarnorParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void NorvosParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void NorvosParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void NorvosParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void NorvosParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void NorvosParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void NorvosParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void ValyrianParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void ValyrianParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void ValyrianParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void ValyrianParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void ValyrianParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void ValyrianParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void VolantineParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void VolantineParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void VolantineParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void VolantineParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void VolantineParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void VolantineParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void QohorikParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    protected void QohorikParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    protected void QohorikParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    protected void QohorikParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    protected void QohorikParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    protected void QohorikParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    protected void FreeFolkParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void FreeFolkParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void FreeFolkParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void FreeFolkParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void FreeFolkParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void FreeFolkParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NightsWatchParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NightsWatchParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NightsWatchParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NightsWatchParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NightsWatchParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NightsWatchParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BattanianParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BattanianParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BattanianParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BattanianParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BattanianParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BattanianParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BoltonParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BoltonParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BoltonParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BoltonParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BoltonParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void BoltonParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SturgiaParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SturgiaParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SturgiaParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SturgiaParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SturgiaParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SturgiaParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValeParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValeParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValeParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValeParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValeParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValeParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void RiverlandsParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void RiverlandsParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void RiverlandsParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void RiverlandsParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void RiverlandsParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void RiverlandsParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VlandiaParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VlandiaParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VlandiaParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VlandiaParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VlandiaParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VlandiaParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void DragonstoneParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void DragonstoneParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void DragonstoneParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void DragonstoneParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void DragonstoneParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void DragonstoneParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void StormlandsParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void StormlandsParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void StormlandsParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void StormlandsParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void StormlandsParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void StormlandsParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ReachParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ReachParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ReachParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ReachParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ReachParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ReachParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void CrownlandsParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void CrownlandsParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void CrownlandsParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void CrownlandsParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void CrownlandsParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void CrownlandsParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void AseraiParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void AseraiParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void AseraiParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void AseraiParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void AseraiParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void AseraiParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void EmpireParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void EmpireParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void EmpireParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void EmpireParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void EmpireParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void EmpireParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void GhiscariParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void GhiscariParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void GhiscariParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void GhiscariParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void GhiscariParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void GhiscariParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void KhuzaitParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void KhuzaitParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void KhuzaitParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void KhuzaitParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void KhuzaitParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void KhuzaitParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SarnorParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SarnorParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SarnorParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SarnorParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SarnorParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void SarnorParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NorvosParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NorvosParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NorvosParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NorvosParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NorvosParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void NorvosParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValyrianParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValyrianParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValyrianParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValyrianParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValyrianParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void ValyrianParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VolantineParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VolantineParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VolantineParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VolantineParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VolantineParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void VolantineParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void QohorikParentsOption1OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void QohorikParentsOption2OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void QohorikParentsOption3OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void QohorikParentsOption4OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void QohorikParentsOption5OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected void QohorikParentsOption6OnApply(CharacterCreation characterCreation)
    {
        FinalizeParents();
    }

    protected bool FreefolkParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "freefolk";
    }

    protected bool NightsWatchParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "nightswatch";
    }

    protected bool BattanianParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "battania";
    }

    protected bool BoltonParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "bolton";
    }

    protected bool SturgiaParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "sturgia";
    }

    protected bool ValeParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "vale";
    }

    protected bool RiverlandsParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "river";
    }

    protected bool VlandiaParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "vlandia";
    }

    protected bool DragonstoneParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "dragonstone";
    }

    protected bool StormlandsParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "stormlands";
    }

    protected bool ReachParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "reach";
    }

    protected bool CrownlandsParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "crownlands";
    }

    protected bool AseraiParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "aserai";
    }

    protected bool EmpireParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "empire";
    }

    protected bool GhiscariParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "ghiscari";
    }

    protected bool KhuzaitParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "khuzait";
    }

    protected bool SarnorParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "sarnor";
    }

    protected bool NorvosParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "norvos";
    }

    protected bool ValyrianParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "valyrian";
    }

    protected bool VolantineParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "volantine";
    }

    protected bool QohorikParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "qohorik";
    }

    protected void FinalizeParents()
    {
        var @object = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_mother");
        var object2 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_father");
        @object.HeroObject.ModifyPlayersFamilyAppearance(MotherFacegenCharacter.BodyProperties.StaticProperties);
        object2.HeroObject.ModifyPlayersFamilyAppearance(FatherFacegenCharacter.BodyProperties.StaticProperties);
        @object.HeroObject.Weight = MotherFacegenCharacter.BodyProperties.Weight;
        @object.HeroObject.Build = MotherFacegenCharacter.BodyProperties.Build;
        object2.HeroObject.Weight = FatherFacegenCharacter.BodyProperties.Weight;
        object2.HeroObject.Build = FatherFacegenCharacter.BodyProperties.Build;
        EquipmentHelper.AssignHeroEquipmentFromEquipment(@object.HeroObject, MotherFacegenCharacter.Equipment);
        EquipmentHelper.AssignHeroEquipmentFromEquipment(object2.HeroObject, FatherFacegenCharacter.Equipment);
        @object.Culture = Hero.MainHero.Culture;
        object2.Culture = Hero.MainHero.Culture;
        StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter);
        var textObject = GameTexts.FindText("str_player_father_name", Hero.MainHero.Culture.StringId);
        object2.HeroObject.SetName(textObject, textObject);
        var textObject2 = new TextObject("{=XmvaRfLM}{PLAYER_FATHER.NAME} was the father of {PLAYER.LINK}. He was slain when raiders attacked the inn at which his family was staying.");
        StringHelpers.SetCharacterProperties("PLAYER_FATHER", object2, textObject2);
        object2.HeroObject.EncyclopediaText = textObject2;
        var textObject3 = GameTexts.FindText("str_player_mother_name", Hero.MainHero.Culture.StringId);
        @object.HeroObject.SetName(textObject3, textObject3);
        var textObject4 = new TextObject("{=hrhvEWP8}{PLAYER_MOTHER.NAME} was the mother of {PLAYER.LINK}. She was slain when raiders attacked the inn at which her family was staying.");
        StringHelpers.SetCharacterProperties("PLAYER_MOTHER", @object, textObject4);
        @object.HeroObject.EncyclopediaText = textObject4;
        @object.HeroObject.UpdateHomeSettlement();
        object2.HeroObject.UpdateHomeSettlement();
        @object.HeroObject.HasMet = true;
        object2.HeroObject.HasMet = true;
    }

    protected static List<FaceGenChar> ChangePlayerFaceWithAge(float age, string actionName = "act_childhood_schooled")
    {
        var list = new List<FaceGenChar>();
        var originalBodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment);
        originalBodyProperties = FaceGen.GetBodyPropertiesWithAge(ref originalBodyProperties, age);
        list.Add(new FaceGenChar(originalBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), CharacterObject.PlayerCharacter.IsFemale, actionName));
        return list;
    }

    protected Equipment ChangePlayerOutfit(CharacterCreation characterCreation, string outfit)
    {
        var list = new List<Equipment>();
        var equipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(outfit)?.DefaultEquipment ?? Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_default").DefaultEquipment;
        list.Add(equipment);
        characterCreation.ChangeCharactersEquipment(list);
        return equipment;
    }

    protected static void ChangePlayerMount(CharacterCreation characterCreation, Hero hero)
    {
        if (hero.CharacterObject.HasMount())
        {
            var faceGenMount = new FaceGenMount(MountCreationKey.GetRandomMountKey(hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, hero.CharacterObject.GetMountKeySeed()), hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, hero.CharacterObject.Equipment[EquipmentIndex.HorseHarness].Item, "act_horse_stand_1");
            characterCreation.SetFaceGenMount(faceGenMount);
        }
    }

    protected static void ClearMountEntity(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenMounts();
    }

    protected void ChildhoodOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(ChildhoodAge));
        var outfit = "player_char_creation_childhood_age_" + GetSelectedCulture().StringId + "_" + SelectedParentType + (Hero.MainHero.IsFemale ? "_f" : "_m");
        ChangePlayerOutfit(characterCreation, outfit);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        ClearMountEntity(characterCreation);
    }

    protected void EducationOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        var textObject = new TextObject("{=WYvnWcXQ}Like all village children you helped out in the fields. You also...");
        var textObject2 = new TextObject("{=DsCkf6Pb}Growing up, you spent most of your time...");
        EducationIntroductoryText.SetTextVariable("EDUCATION_INTRO", RuralType() ? textObject : textObject2);
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(EducationAge));
        var outfit = "player_char_creation_education_age_" + GetSelectedCulture().StringId + "_" + SelectedParentType + (Hero.MainHero.IsFemale ? "_f" : "_m");
        ChangePlayerOutfit(characterCreation, outfit);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        ClearMountEntity(characterCreation);
    }

    protected bool RuralType()
    {
        return FamilyOccupationType == OccupationTypes.Retainer || FamilyOccupationType == OccupationTypes.Farmer || FamilyOccupationType == OccupationTypes.Hunter || FamilyOccupationType == OccupationTypes.Bard || FamilyOccupationType == OccupationTypes.Herder || FamilyOccupationType == OccupationTypes.Vagabond || FamilyOccupationType == OccupationTypes.Healer || FamilyOccupationType == OccupationTypes.Artisan;
    }

    protected bool RichParents()
    {
        return FamilyOccupationType == OccupationTypes.Retainer || FamilyOccupationType == OccupationTypes.Merchant;
    }

    protected bool RuralAdolescenceOnCondition()
    {
        return RuralType();
    }

    protected bool UrbanAdolescenceOnCondition()
    {
        return !RuralType();
    }

    protected bool UrbanRichAdolescenceOnCondition()
    {
        return !RuralType() && RichParents();
    }

    protected bool UrbanPoorAdolescenceOnCondition()
    {
        return !RuralType() && !RichParents();
    }

    protected void RefreshPropsAndClothing(CharacterCreation characterCreation, bool isChildhoodStage, string itemId, bool isLeftHand, string secondItemId = "")
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ClearCharactersEquipment();
        var text = isChildhoodStage ? "player_char_creation_childhood_age_" + GetSelectedCulture().StringId + "_" + SelectedParentType : "player_char_creation_education_age_" + GetSelectedCulture().StringId + "_" + SelectedParentType;
        var outfit = text + (Hero.MainHero.IsFemale ? "_f" : "_m");
        var equipment = ChangePlayerOutfit(characterCreation, outfit).Clone();
        if (Game.Current.ObjectManager.GetObject<ItemObject>(itemId) != null)
        {
            var @object = Game.Current.ObjectManager.GetObject<ItemObject>(itemId);
            equipment.AddEquipmentToSlotWithoutAgent(!isLeftHand ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(@object));
            if (secondItemId != "")
            {
                var object2 = Game.Current.ObjectManager.GetObject<ItemObject>(secondItemId);
                equipment.AddEquipmentToSlotWithoutAgent(isLeftHand ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(object2));
            }
        }
        else
        {
            var baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCreation.FaceGenChars[0].Race);
            characterCreation.ChangeCharacterPrefab(itemId, isLeftHand ? baseMonsterFromRace.MainHandItemBoneIndex : baseMonsterFromRace.OffHandItemBoneIndex);
        }

        characterCreation.ChangeCharactersEquipment(new List<Equipment> { equipment });
    }

    protected void YouthOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        var textObject = new TextObject("{=F7OO5SAa}As a youngster growing up in Calradia, war was never too far away. You...");
        var textObject2 = new TextObject("{=5kbeAC7k}In wartorn Calradia, especially in frontier or tribal areas, some women as well as men learn to fight from an early age. You...");
        YouthIntroductoryText.SetTextVariable("YOUTH_INTRO", CharacterObject.PlayerCharacter.IsFemale ? textObject2 : textObject);
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(YouthAge));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        if (SelectedTitleType < 1 || SelectedTitleType > 10) SelectedTitleType = 1;
        RefreshPlayerAppearance(characterCreation);
    }

    protected void RefreshPlayerAppearance(CharacterCreation characterCreation)
    {
        var outfit = "player_char_creation_" + GetSelectedCulture().StringId + "_" + SelectedTitleType + (Hero.MainHero.IsFemale ? "_f" : "_m");
        ChangePlayerOutfit(characterCreation, outfit);
        ApplyEquipments(characterCreation);
    }

    protected void AccomplishmentOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(AccomplishmentAge));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        RefreshPlayerAppearance(characterCreation);
    }

    protected void StartingAgeOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge((float)StartingAge));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        RefreshPlayerAppearance(characterCreation);
    }

    protected void StartingAgeYoungOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(20f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_focus" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.YoungAdult;
        SetHeroAge(20f);
    }

    protected void StartingAgeAdultOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(30f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_ready" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.Adult;
        SetHeroAge(30f);
    }

    protected void StartingAgeMiddleAgedOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(40f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.MiddleAged;
        SetHeroAge(40f);
    }

    protected void StartingAgeElderlyOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(50f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_tough" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.Elder;
        SetHeroAge(50f);
    }

    protected void StartingAgeYoungOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.YoungAdult;
    }

    protected void StartingAgeAdultOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.Adult;
    }

    protected void StartingAgeMiddleAgedOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.MiddleAged;
    }

    protected void StartingAgeElderlyOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.Elder;
    }

    protected void ApplyEquipments(CharacterCreation characterCreation)
    {
        ClearMountEntity(characterCreation);
        var @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_" + GetSelectedCulture().StringId + "_" + SelectedTitleType + (Hero.MainHero.IsFemale ? "_f" : "_m"));
        PlayerStartEquipment = @object?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
        PlayerCivilianEquipment = @object?.GetCivilianEquipments().FirstOrDefault() ?? MBEquipmentRoster.EmptyEquipment;
        if (PlayerStartEquipment != null && PlayerCivilianEquipment != null)
        {
            CharacterObject.PlayerCharacter.Equipment.FillFrom(PlayerStartEquipment);
            CharacterObject.PlayerCharacter.FirstCivilianEquipment.FillFrom(PlayerCivilianEquipment);
        }

        ChangePlayerMount(characterCreation, Hero.MainHero);
    }

    protected void SetHeroAge(float age)
    {
        Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(0f - age));
    }
}
