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

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

public class ROTCharacterCreationContent : CharacterCreationContentBase
{
    private enum SandboxAgeOptions
    {
        YoungAdult = 20,
        Adult = 30,
        MiddleAged = 40,
        Elder = 50
    }

    private enum OccupationTypes
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

    private readonly Dictionary<string, Vec2> StartingPoints = new()
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
        }
    };

    private SandboxAgeOptions StartingAge = SandboxAgeOptions.YoungAdult;
    private OccupationTypes FamilyOccupationType;
    private readonly TextObject EducationIntroductoryText = new TextObject("{=!}{EDUCATION_INTRO}");
    private readonly TextObject YouthIntroductoryText = new TextObject("{=!}{YOUTH_INTRO}");
    public override TextObject ReviewPageDescription => new TextObject("{=W6pKpEoT}You prepare to set off for a grand adventure in Alantia! Here is your character. Continue if you are ready, or go back to make changes.");

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
        base.SelectedTitleType = 1;
        base.SelectedParentType = 0;
    }

    public override int GetSelectedParentType() => base.SelectedParentType;

    public override void OnCharacterCreationFinalized()
    {
        var culture = CharacterObject.PlayerCharacter.Culture;
        if (StartingPoints.TryGetValue(culture.StringId, out var position2D))
        {
            MobileParty.MainParty.Position2D = position2D;
        }
        else
        {
            MobileParty.MainParty.Position2D = Campaign.Current.DefaultStartingPosition;
        }

        if (GameStateManager.Current.ActiveState is MapState mapState)
        {
            mapState.Handler.ResetCamera(resetDistance: true, teleportToMainParty: true);
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

    private void AddParentsMenu(CharacterCreation characterCreation)
    {
        var familyMenu = new CharacterCreationMenu(new TextObject("{=b4lDDcli}Family"), new TextObject("{=XgFU1pCx}You were born into a family of..."), ParentsOnInit);
        var freefolk = familyMenu.AddMenuCategory(FreefolkParentsOnCondition);
        freefolk.AddCategoryOption(new TextObject("{FreParO1}Freefolk Warriors"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption1OnConsequence, FinalizeParents, new TextObject("{FreParT1}"));
        freefolk.AddCategoryOption(new TextObject("{FreParO2}Frostfang Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption2OnConsequence, FinalizeParents, new TextObject("{FreParT2}"));
        freefolk.AddCategoryOption(new TextObject("{FreParO3}Thenn Cannibals"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption3OnConsequence, FinalizeParents, new TextObject("{FreParT3}"));
        freefolk.AddCategoryOption(new TextObject("{FreParO4}Haunted Forest Hunters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Trade
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption4OnConsequence, FinalizeParents, new TextObject("{FreParT4}"));
        freefolk.AddCategoryOption(new TextObject("{FreParO1}Shamans"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption5OnConsequence, FinalizeParents, new TextObject("{FreParT5}"));
        freefolk.AddCategoryOption(new TextObject("{FreParO6}Bards"), new List<SkillObject>
        {
            DefaultSkills.Charm,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, FreeFolkParentsOption6OnConsequence, FinalizeParents, new TextObject("{FreParT6}"));
        var nightswatch = familyMenu.AddMenuCategory(NightsWatchParentsOnCondition);
        nightswatch.AddCategoryOption(new TextObject("{NiPareO1}Castle Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption1OnConsequence, FinalizeParents, new TextObject("{NiPareT1}"));
        nightswatch.AddCategoryOption(new TextObject("{NiPareO2}Tavern Owners"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption2OnConsequence, FinalizeParents, new TextObject("{NiPareT2}"));
        nightswatch.AddCategoryOption(new TextObject("{NiPareO3}Maester's Apprentices"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption3OnConsequence, FinalizeParents, new TextObject("{NiPareT3}"));
        nightswatch.AddCategoryOption(new TextObject("{NiPareO4}Brigands"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Crossbow
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption4OnConsequence, FinalizeParents, new TextObject("{NiPareT4}"));
        nightswatch.AddCategoryOption(new TextObject("{NiPareO5}Freeriders"), new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption5OnConsequence, FinalizeParents, new TextObject("{NiPareT5}"));
        nightswatch.AddCategoryOption(new TextObject("{NiPareO6}Foresters"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, NightsWatchParentsOption6OnConsequence, FinalizeParents, new TextObject("{NiPareT6}"));
        var battania = familyMenu.AddMenuCategory(BattanianParentsOnCondition);
        battania.AddCategoryOption(new TextObject("{BatParO1}Stark Men at Arms"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption1OnConsequence, FinalizeParents, new TextObject("{BatParT1}"));
        battania.AddCategoryOption(new TextObject("{BatParO2}Hunters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption2OnConsequence, FinalizeParents, new TextObject("{BatParT2}"));
        battania.AddCategoryOption(new TextObject("{BatParO3}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption3OnConsequence, FinalizeParents, new TextObject("{BatParT3}"));
        battania.AddCategoryOption(new TextObject("{BatParO4}Blacksmiths"), new List<SkillObject>
        {
            DefaultSkills.Crafting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption4OnConsequence, FinalizeParents, new TextObject("{BatParT4}"));
        battania.AddCategoryOption(new TextObject("{BatParO5}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Medicine
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption5OnConsequence, FinalizeParents, new TextObject("{BatParT5}"));
        battania.AddCategoryOption(new TextObject("{BatParO6}Outlaws"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BattanianParentsOption6OnConsequence, FinalizeParents, new TextObject("{BatParT6}"));
        var bolton = familyMenu.AddMenuCategory(BoltonParentsOnCondition);
        bolton.AddCategoryOption(new TextObject("{BolParO1}Dreafort Gatemen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption1OnConsequence, FinalizeParents, new TextObject("{BolParT1}"));
        bolton.AddCategoryOption(new TextObject("{BolParO2}Frey Crossbowmen"), new List<SkillObject>
        {
            DefaultSkills.Crossbow,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption2OnConsequence, FinalizeParents, new TextObject("{BolParT2}"));
        bolton.AddCategoryOption(new TextObject("{BolParO3}Karstark Vanguard"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption3OnConsequence, FinalizeParents, new TextObject("{BolParT3}"));
        bolton.AddCategoryOption(new TextObject("{BolParO4}Forest Bandits"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption4OnConsequence, FinalizeParents, new TextObject("{BolParT4}"));
        bolton.AddCategoryOption(new TextObject("{BolParO5}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption5OnConsequence, FinalizeParents, new TextObject("{BolParT5}"));
        bolton.AddCategoryOption(new TextObject("{BolParO6}Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, BoltonParentsOption6OnConsequence, FinalizeParents, new TextObject("{BolParT6}"));
        var ironborn = familyMenu.AddMenuCategory(SturgiaParentsOnCondition);
        ironborn.AddCategoryOption(new TextObject("{StuParO1}Greyjoy Footmen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption1OnConsequence, FinalizeParents, new TextObject("{StuParT1}"));
        ironborn.AddCategoryOption(new TextObject("{StuParO2}Ironborn Reavers"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Roguery
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption2OnConsequence, FinalizeParents, new TextObject("{StuParT2}"));
        ironborn.AddCategoryOption(new TextObject("{StuParO3}Fishermen"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption3OnConsequence, FinalizeParents, new TextObject("{StuParT3}"));
        ironborn.AddCategoryOption(new TextObject("{StuParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption4OnConsequence, FinalizeParents, new TextObject("{StuParT4}"));
        ironborn.AddCategoryOption(new TextObject("{StuParO5}Drowned Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption5OnConsequence, FinalizeParents, new TextObject("{StuParT5}"));
        ironborn.AddCategoryOption(new TextObject("{StuParO6}Deckhands"), new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, SturgiaParentsOption6OnConsequence, FinalizeParents, new TextObject("{StuParT6}"));
        var vale = familyMenu.AddMenuCategory(ValeParentsOnCondition);
        vale.AddCategoryOption(new TextObject("{ValParO1}Arryn Castle Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption1OnConsequence, FinalizeParents, new TextObject("{ValParT1}"));
        vale.AddCategoryOption(new TextObject("{ValParO2}Knights of the Vale"), new List<SkillObject>
        {
            DefaultSkills.Polearm,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption2OnConsequence, FinalizeParents, new TextObject("{ValParT2}"));
        vale.AddCategoryOption(new TextObject("{ValParO3}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption3OnConsequence, FinalizeParents, new TextObject("{ValParT3}"));
        vale.AddCategoryOption(new TextObject("{ValParO4}Mountains of the Moon Hunters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption4OnConsequence, FinalizeParents, new TextObject("{ValParT4}"));
        vale.AddCategoryOption(new TextObject("{ValParO5}Brigands"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Throwing
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption5OnConsequence, FinalizeParents, new TextObject("{ValParT5}"));
        vale.AddCategoryOption(new TextObject("{ValParO6}Stonemasons"), new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ValeParentsOption6OnConsequence, FinalizeParents, new TextObject("{ValParT6}"));
        var river = familyMenu.AddMenuCategory(RiverlandsParentsOnCondition);
        river.AddCategoryOption(new TextObject("{RivParO1}Tully Men at Arms"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption1OnConsequence, FinalizeParents, new TextObject("{RivParT1}"));
        river.AddCategoryOption(new TextObject("{RivParO2}Tavern Owners"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption2OnConsequence, FinalizeParents, new TextObject("{RivParT2}"));
        river.AddCategoryOption(new TextObject("{RivParO3}River Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption3OnConsequence, FinalizeParents, new TextObject("{RivParT3}"));
        river.AddCategoryOption(new TextObject("{RivParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Medicine
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption4OnConsequence, FinalizeParents, new TextObject("{RivParT4}"));
        river.AddCategoryOption(new TextObject("{RivParO5}River Pirates"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Crossbow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption5OnConsequence, FinalizeParents, new TextObject("{RivParT5}"));
        river.AddCategoryOption(new TextObject("{RivParO6}Mallister Knights"), new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, RiverlandsParentsOption6OnConsequence, FinalizeParents, new TextObject("{RivParT6}"));
        var vlandia = familyMenu.AddMenuCategory(VlandiaParentsOnCondition);
        vlandia.AddCategoryOption(new TextObject("{VlaParO1}Lannister Soldiers"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption1OnConsequence, FinalizeParents, new TextObject("{VlaParT1}"));
        vlandia.AddCategoryOption(new TextObject("{VlaParO2}Miners"), new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption2OnConsequence, FinalizeParents, new TextObject("{VlaParT2}"));
        vlandia.AddCategoryOption(new TextObject("{VlaParO3}Shop Clerks"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption3OnConsequence, FinalizeParents, new TextObject("{VlaParT3}"));
        vlandia.AddCategoryOption(new TextObject("{VlaParO4}City Thugs"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption4OnConsequence, FinalizeParents, new TextObject("{VlaParT4}"));
        vlandia.AddCategoryOption(new TextObject("{VlaParO5}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Medicine
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption5OnConsequence, FinalizeParents, new TextObject("{VlaParT5}"));
        vlandia.AddCategoryOption(new TextObject("{VlaParO6}Hill Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, VlandiaParentsOption6OnConsequence, FinalizeParents, new TextObject("{VlaParT6}"));
        var dragonstone = familyMenu.AddMenuCategory(DragonstoneParentsOnCondition);
        dragonstone.AddCategoryOption(new TextObject("{DraParO1}Dragonstone Castle Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption1OnConsequence, FinalizeParents, new TextObject("{DraParT1}"));
        dragonstone.AddCategoryOption(new TextObject("{DraParO2}Blackwater Fishermen"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption2OnConsequence, FinalizeParents, new TextObject("{DraParT2}"));
        dragonstone.AddCategoryOption(new TextObject("{DraParO3}Red Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption3OnConsequence, FinalizeParents, new TextObject("{DraParT3}"));
        dragonstone.AddCategoryOption(new TextObject("{DraParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption4OnConsequence, FinalizeParents, new TextObject("{DraParT4}"));
        dragonstone.AddCategoryOption(new TextObject("{DraParO5}Bandit Sailors"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption5OnConsequence, FinalizeParents, new TextObject("{DraParT5}"));
        dragonstone.AddCategoryOption(new TextObject("{DraParO6}Castle Wall Sharpshooters"), new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Crossbow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, DragonstoneParentsOption6OnConsequence, FinalizeParents, new TextObject("{DraParT6}"));
        var stormlands = familyMenu.AddMenuCategory(StormlandsParentsOnCondition);
        stormlands.AddCategoryOption(new TextObject("{StoParO1}Baratheon Footmen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption1OnConsequence, FinalizeParents, new TextObject("{StoParT1}"));
        stormlands.AddCategoryOption(new TextObject("{StoParO2}Blacksmiths"), new List<SkillObject>
        {
            DefaultSkills.Crafting,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption2OnConsequence, FinalizeParents, new TextObject("{StoParT2}"));
        stormlands.AddCategoryOption(new TextObject("{StoParO3}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption3OnConsequence, FinalizeParents, new TextObject("{StoParT3}"));
        stormlands.AddCategoryOption(new TextObject("{StoParO4}Maester's Apprentice"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption4OnConsequence, FinalizeParents, new TextObject("{StoParT4}"));
        stormlands.AddCategoryOption(new TextObject("{StoParO5}Kingswood Brigands"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption5OnConsequence, FinalizeParents, new TextObject("{StoParT5}"));
        stormlands.AddCategoryOption(new TextObject("{StoParO6}Mistwood Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, StormlandsParentsOption6OnConsequence, FinalizeParents, new TextObject("{StoParT6}"));
        var reach = familyMenu.AddMenuCategory(ReachParentsOnCondition);
        reach.AddCategoryOption(new TextObject("{ReaParO1}Tyrell House Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption1OnConsequence, FinalizeParents, new TextObject("{ReaParT1}"));
        reach.AddCategoryOption(new TextObject("{ReaParO2}Farmers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption2OnConsequence, FinalizeParents, new TextObject("{ReaParT2}"));
        reach.AddCategoryOption(new TextObject("{ReaParO3}Tarly Horsemen"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption3OnConsequence, FinalizeParents, new TextObject("{ReaParT3}"));
        reach.AddCategoryOption(new TextObject("{ReaParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption4OnConsequence, FinalizeParents, new TextObject("{ReaParT4}"));
        reach.AddCategoryOption(new TextObject("{ReaParO5}Oldtown Acolyte"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption5OnConsequence, FinalizeParents, new TextObject("{ReaParT5}"));
        reach.AddCategoryOption(new TextObject("{ReaParO6}Rogues"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, ReachParentsOption6OnConsequence, FinalizeParents, new TextObject("{ReaParT6}"));
        var crownlands = familyMenu.AddMenuCategory(CrownlandsParentsOnCondition);
        crownlands.AddCategoryOption(new TextObject("{CroParO1}Goldcloaks"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption1OnConsequence, FinalizeParents, new TextObject("{CroParT1}"));
        crownlands.AddCategoryOption(new TextObject("{CroParO2}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption2OnConsequence, FinalizeParents, new TextObject("{CroParT2}"));
        crownlands.AddCategoryOption(new TextObject("{CroParO3}Faith Militant Acolytes"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.OneHanded
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption3OnConsequence, FinalizeParents, new TextObject("{CroParT3}"));
        crownlands.AddCategoryOption(new TextObject("{CroParO4}Flea Bottom Thugs"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption4OnConsequence, FinalizeParents, new TextObject("{CroParT4}"));
        crownlands.AddCategoryOption(new TextObject("{CroParO5}Brothel Owners"), new List<SkillObject>
        {
            DefaultSkills.Charm,
            DefaultSkills.Trade
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption5OnConsequence, FinalizeParents, new TextObject("{CroParT5}"));
        crownlands.AddCategoryOption(new TextObject("{CroParO6}Stonemason"), new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, CrownlandsParentsOption6OnConsequence, FinalizeParents, new TextObject("{CroParT6}"));
        var aserai = familyMenu.AddMenuCategory(AseraiParentsOnCondition);
        aserai.AddCategoryOption(new TextObject("{AseParO1}Martell Palace Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption1OnConsequence, FinalizeParents, new TextObject("{AseParT1}"));
        aserai.AddCategoryOption(new TextObject("{AseParO2}Caravan Masters"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Scouting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption2OnConsequence, FinalizeParents, new TextObject("{AseParT2}"));
        aserai.AddCategoryOption(new TextObject("{AseParO3}Desert Bandits"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption3OnConsequence, FinalizeParents, new TextObject("{AseParT3}"));
        aserai.AddCategoryOption(new TextObject("{AseParO4}Castellans"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption4OnConsequence, FinalizeParents, new TextObject("{AseParT4}"));
        aserai.AddCategoryOption(new TextObject("{AseParO5}Horse Farmers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption5OnConsequence, FinalizeParents, new TextObject("{AseParT5}"));
        aserai.AddCategoryOption(new TextObject("{AseParO6}Maester's Apprentice"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, AseraiParentsOption6OnConsequence, FinalizeParents, new TextObject("{AseParT6}"));
        var empire = familyMenu.AddMenuCategory(EmpireParentsOnCondition);
        empire.AddCategoryOption(new TextObject("{EmpParO1}Volantine City Guard"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption1OnConsequence, FinalizeParents, new TextObject("{EmpParT1}"));
        empire.AddCategoryOption(new TextObject("{EmpParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption2OnConsequence, FinalizeParents, new TextObject("{EmpParT2}"));
        empire.AddCategoryOption(new TextObject("{EmpParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption3OnConsequence, FinalizeParents, new TextObject("{EmpParT3}"));
        empire.AddCategoryOption(new TextObject("{EmpParO4}Red Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption4OnConsequence, FinalizeParents, new TextObject("{EmpParT4}"));
        empire.AddCategoryOption(new TextObject("{EmpParO5}Summer Sea Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption5OnConsequence, FinalizeParents, new TextObject("{EmpParT5}"));
        empire.AddCategoryOption(new TextObject("{EmpParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption6OnConsequence, FinalizeParents, new TextObject("{EmpParT6}"));
        var ghiscari = familyMenu.AddMenuCategory(GhiscariParentsOnCondition);
        ghiscari.AddCategoryOption(new TextObject("{GhiParO1}Pyramid Guards"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption1OnConsequence, FinalizeParents, new TextObject("{GhiParT1}"));
        ghiscari.AddCategoryOption(new TextObject("{GhiParO2}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption2OnConsequence, FinalizeParents, new TextObject("{GhiParT2}"));
        ghiscari.AddCategoryOption(new TextObject("{GhiParO3}Sons of the Harpy"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption3OnConsequence, FinalizeParents, new TextObject("{GhiParT3}"));
        ghiscari.AddCategoryOption(new TextObject("{GhiParO4}Shop Owners"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption4OnConsequence, FinalizeParents, new TextObject("{GhiParT4}"));
        ghiscari.AddCategoryOption(new TextObject("{GhiParO5}Wise Masters"), new List<SkillObject>
        {
            DefaultSkills.Steward,
            DefaultSkills.Engineering
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption5OnConsequence, FinalizeParents, new TextObject("{GhiParT5}"));
        ghiscari.AddCategoryOption(new TextObject("{GhiParO6}Mercenary Scouts"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, GhiscariParentsOption6OnConsequence, FinalizeParents, new TextObject("{GhiParT6}"));
        var khuzait = familyMenu.AddMenuCategory(KhuzaitParentsOnCondition);
        khuzait.AddCategoryOption(new TextObject("{KhuParO1}Dothraki Warriors"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption1OnConsequence, FinalizeParents, new TextObject("{KhuParT1}"));
        khuzait.AddCategoryOption(new TextObject("{KhuParO2}Dothraki Bowmen"), new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Bow
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption2OnConsequence, FinalizeParents, new TextObject("{KhuParT2}"));
        khuzait.AddCategoryOption(new TextObject("{KhuParO3}Slave"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption3OnConsequence, FinalizeParents, new TextObject("{KhuParT3}"));
        khuzait.AddCategoryOption(new TextObject("{KhuParO4}Village Herder"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption4OnConsequence, FinalizeParents, new TextObject("{KhuParT4}"));
        khuzait.AddCategoryOption(new TextObject("{KhuParO5}Steppe Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Riding
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption5OnConsequence, FinalizeParents, new TextObject("{KhuParT5}"));
        khuzait.AddCategoryOption(new TextObject("{KhuParO6}Village Warlocks"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, KhuzaitParentsOption6OnConsequence, FinalizeParents, new TextObject("{KhuParT6}"));
        var sarnor = familyMenu.AddMenuCategory(SarnorParentsOnCondition);
        sarnor.AddCategoryOption(new TextObject("{SarParO1}Saath City Guard"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption1OnConsequence, FinalizeParents, new TextObject("{SarParT1}"));
        sarnor.AddCategoryOption(new TextObject("{SarParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption2OnConsequence, FinalizeParents, new TextObject("{SarParT2}"));
        sarnor.AddCategoryOption(new TextObject("{SarParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption3OnConsequence, FinalizeParents, new TextObject("{SarParT3}"));
        sarnor.AddCategoryOption(new TextObject("{SarParO4}Ankasi Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption4OnConsequence, FinalizeParents, new TextObject("{SarParT4}"));
        sarnor.AddCategoryOption(new TextObject("{SarParO5}Shivering Sea Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption5OnConsequence, FinalizeParents, new TextObject("{SarParT5}"));
        sarnor.AddCategoryOption(new TextObject("{SarParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption6OnConsequence, FinalizeParents, new TextObject("{SarParT6}"));
        var norvos = familyMenu.AddMenuCategory(NorvosParentsOnCondition);
        norvos.AddCategoryOption(new TextObject("{NorParO1}Norvos City Guard"), new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm
        }, DefaultCharacterAttributes.Vigor, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption1OnConsequence, FinalizeParents, new TextObject("{NorParT1}"));
        norvos.AddCategoryOption(new TextObject("{NorParO2}Slavers"), new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.Tactics
        }, DefaultCharacterAttributes.Cunning, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption2OnConsequence, FinalizeParents, new TextObject("{NorParT2}"));
        norvos.AddCategoryOption(new TextObject("{NorParO3}Tavern Keepers"), new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        }, DefaultCharacterAttributes.Social, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption3OnConsequence, FinalizeParents, new TextObject("{NorParT3}"));
        norvos.AddCategoryOption(new TextObject("{NorParO4}Bearded Priests"), new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Steward
        }, DefaultCharacterAttributes.Intelligence, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption4OnConsequence, FinalizeParents, new TextObject("{NorParT4}"));
        norvos.AddCategoryOption(new TextObject("{NorParO5}Noyne Sailors"), new List<SkillObject>
        {
            DefaultSkills.Scouting,
            DefaultSkills.Athletics
        }, DefaultCharacterAttributes.Control, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption5OnConsequence, FinalizeParents, new TextObject("{NorParT5}"));
        norvos.AddCategoryOption(new TextObject("{NorParO6}Slaves"), new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        }, DefaultCharacterAttributes.Endurance, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, EmpireParentsOption6OnConsequence, FinalizeParents, new TextObject("{NorParT6}"));
        characterCreation.AddNewMenu(familyMenu);
    }

    private void AddChildhoodMenu(CharacterCreation characterCreation)
    {
        var childhoodMenu = new CharacterCreationMenu(new TextObject("{=8Yiwt1z6}Early Childhood"), new TextObject("{=character_creation_content_16}As a child you were noted for..."), ChildhoodOnInit);
        var creationCategory = childhoodMenu.AddMenuCategory();
        var text1 = new TextObject("{=kmM68Qx4}your leadership skills.");
        var effectedSkills1 = new List<SkillObject>
        {
            DefaultSkills.Leadership,
            DefaultSkills.Tactics
        };
        var cunning = DefaultCharacterAttributes.Cunning;
        var focusToAdd1 = FocusToAdd;
        var skillLevelToAdd1 = SkillLevelToAdd;
        var attributeLevelToAdd1 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect1 = ChildhoodYourLeadershipSkillsOnConsequence;
        var descriptionText1 = new TextObject("{=FfNwXtii}If the wolf pup gang of your early childhood had an alpha, it was definitely you. All the other kids followed your lead as you decided what to play and where to play, and led them in games and mischief.");
        creationCategory.AddCategoryOption(text1, effectedSkills1, cunning, focusToAdd1, skillLevelToAdd1, attributeLevelToAdd1, null, onSelect1, null, descriptionText1);
        var text2 = new TextObject("{=5HXS8HEY}your brawn.");
        var effectedSkills2 = new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Throwing
        };
        var vigor = DefaultCharacterAttributes.Vigor;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect2 = ChildhoodYourBrawnOnConsequence;
        var descriptionText2 = new TextObject("{=YKzuGc54}You were big, and other children looked to have you around in any scrap with children from a neighboring village. You pushed a plough and throw an axe like an adult.");
        creationCategory.AddCategoryOption(text2, effectedSkills2, vigor, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, null, onSelect2, null, descriptionText2);
        var text3 = new TextObject("{=QrYjPUEf}your attention to detail.");
        var effectedSkills3 = new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Bow
        };
        var control = DefaultCharacterAttributes.Control;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect3 = ChildhoodAttentionToDetailOnConsequence;
        var descriptionText3 = new TextObject("{=JUSHAPnu}You were quick on your feet and attentive to what was going on around you. Usually you could run away from trouble, though you could give a good account of yourself in a fight with other children if cornered.");
        creationCategory.AddCategoryOption(text3, effectedSkills3, control, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, null, onSelect3, null, descriptionText3);
        var text4 = new TextObject("{=Y3UcaX74}your aptitude for numbers.");
        var effectedSkills4 = new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Trade
        };
        var intelligence = DefaultCharacterAttributes.Intelligence;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect4 = ChildhoodAptitudeForNumbersOnConsequence;
        var descriptionText4 = new TextObject("{=DFidSjIf}Most children around you had only the most rudimentary education, but you lingered after class to study letters and mathematics. You were fascinated by the marketplace - weights and measures, tallies and accounts, the chatter about profits and losses.");
        creationCategory.AddCategoryOption(text4, effectedSkills4, intelligence, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, null, onSelect4, null, descriptionText4);
        var text5 = new TextObject("{=GEYzLuwb}your way with people.");
        var effectedSkills5 = new List<SkillObject>
        {
            DefaultSkills.Charm,
            DefaultSkills.Leadership
        };
        var social = DefaultCharacterAttributes.Social;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect5 = ChildhoodWayWithPeopleOnConsequence;
        var descriptionText5 = new TextObject("{=w2TEQq26}You were always attentive to other people, good at guessing their motivations. You studied how individuals were swayed, and tried out what you learned from adults on your friends.");
        creationCategory.AddCategoryOption(text5, effectedSkills5, social, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, null, onSelect5, null, descriptionText5);
        var text6 = new TextObject("{=MEgLE2kj}your skill with horses.");
        var effectedSkills6 = new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Medicine
        };
        var endurance = DefaultCharacterAttributes.Endurance;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect6 = ChildhoodSkillsWithHorsesOnConsequence;
        var descriptionText6 = new TextObject("{=ngazFofr}You were always drawn to animals, and spent as much time as possible hanging out in the village stables. You could calm horses, and were sometimes called upon to break in new colts. You learned the basics of veterinary arts, much of which is applicable to humans as well.");
        creationCategory.AddCategoryOption(text6, effectedSkills6, endurance, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, null, onSelect6, null, descriptionText6);
        characterCreation.AddNewMenu(childhoodMenu);
    }

    private static void ChildhoodYourLeadershipSkillsOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_leader" });
    }

    private static void ChildhoodYourBrawnOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
    }

    private static void ChildhoodAttentionToDetailOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_memory" });
    }

    private static void ChildhoodAptitudeForNumbersOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_numbers" });
    }

    private static void ChildhoodWayWithPeopleOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
    }

    private static void ChildhoodSkillsWithHorsesOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_animals" });
    }

    private void AddEducationMenu(CharacterCreation characterCreation)
    {
        var educationMenu = new CharacterCreationMenu(new TextObject("{=rcoueCmk}Adolescence"), EducationIntroductoryText, EducationOnInit);
        characterCreation.AddNewMenu(educationMenu);
        var creationCategory = educationMenu.AddMenuCategory();
        var text1 = new TextObject("{=RKVNvimC}herded the sheep.");
        var effectedSkills1 = new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Throwing
        };
        var control1 = DefaultCharacterAttributes.Control;
        var focusToAdd1 = FocusToAdd;
        var skillLevelToAdd1 = SkillLevelToAdd;
        var attributeLevelToAdd1 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition1 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect1 = RuralAdolescenceHerderOnConsequence;
        CharacterCreationApplyFinalEffects onApply1 = null;
        var descriptionText1 = new TextObject("{=KfaqPpbK}You went with other fleet-footed youths to take the villages' sheep, goats or cattle to graze in pastures near the village. You were in charge of chasing down stray beasts, and always kept a big stone on hand to be hurled at lurking predators if necessary.");
        creationCategory.AddCategoryOption(text1, effectedSkills1, control1, focusToAdd1, skillLevelToAdd1, attributeLevelToAdd1, optionCondition1, onSelect1, onApply1, descriptionText1);
        var text8 = new TextObject("{=bTKiN0hr}worked in the village smithy.");
        var effectedSkills8 = new List<SkillObject>
        {
            DefaultSkills.TwoHanded,
            DefaultSkills.Crafting
        };
        var vigor1 = DefaultCharacterAttributes.Vigor;
        var focusToAdd8 = FocusToAdd;
        var skillLevelToAdd8 = SkillLevelToAdd;
        var attributeLevelToAdd8 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition8 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect8 = RuralAdolescenceSmithyOnConsequence;
        CharacterCreationApplyFinalEffects onApply8 = null;
        var descriptionText8 = new TextObject("{=y6j1bJTH}You were apprenticed to the local smith. You learned how to heat and forge metal, hammering for hours at a time until your muscles ached.");
        creationCategory.AddCategoryOption(text8, effectedSkills8, vigor1, focusToAdd8, skillLevelToAdd8, attributeLevelToAdd8, optionCondition8, onSelect8, onApply8, descriptionText8);
        var text9 = new TextObject("{=tI8ZLtoA}repaired projects.");
        var effectedSkills9 = new List<SkillObject>
        {
            DefaultSkills.Crafting,
            DefaultSkills.Engineering
        };
        var intelligence1 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd9 = FocusToAdd;
        var skillLevelToAdd9 = SkillLevelToAdd;
        var attributeLevelToAdd9 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition9 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect9 = RuralAdolescenceRepairmanOnConsequence;
        CharacterCreationApplyFinalEffects onApply9 = null;
        var descriptionText9 = new TextObject("{=6LFj919J}You helped dig wells, rethatch houses, and fix broken plows. You learned about the basics of construction, as well as what it takes to keep a farming community prosperous.");
        creationCategory.AddCategoryOption(text9, effectedSkills9, intelligence1, focusToAdd9, skillLevelToAdd9, attributeLevelToAdd9, optionCondition9, onSelect9, onApply9, descriptionText9);
        var text10 = new TextObject("{=TRwgSLD2}gathered herbs in the wild.");
        var effectedSkills10 = new List<SkillObject>
        {
            DefaultSkills.Medicine,
            DefaultSkills.Scouting
        };
        var endurance1 = DefaultCharacterAttributes.Endurance;
        var focusToAdd10 = FocusToAdd;
        var skillLevelToAdd10 = SkillLevelToAdd;
        var attributeLevelToAdd10 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition10 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect10 = RuralAdolescenceGathererOnConsequence;
        CharacterCreationApplyFinalEffects onApply10 = null;
        var descriptionText10 = new TextObject("{=9ks4u5cH}You were sent by the village healer up into the hills to look for useful medicinal plants. You learned which herbs healed wounds or brought down a fever, and how to find them.");
        creationCategory.AddCategoryOption(text10, effectedSkills10, endurance1, focusToAdd10, skillLevelToAdd10, attributeLevelToAdd10, optionCondition10, onSelect10, onApply10, descriptionText10);
        var text11 = new TextObject("{=T7m7ReTq}hunted small game.");
        var effectedSkills11 = new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Tactics
        };
        var control2 = DefaultCharacterAttributes.Control;
        var focusToAdd11 = FocusToAdd;
        var skillLevelToAdd11 = SkillLevelToAdd;
        var attributeLevelToAdd11 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition11 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect11 = RuralAdolescenceHunterOnConsequence;
        CharacterCreationApplyFinalEffects onApply11 = null;
        var descriptionText11 = new TextObject("{=RuvSk3QT}You accompanied a local hunter as he went into the wilderness, helping him set up traps and catch small animals.");
        creationCategory.AddCategoryOption(text11, effectedSkills11, control2, focusToAdd11, skillLevelToAdd11, attributeLevelToAdd11, optionCondition11, onSelect11, onApply11, descriptionText11);
        var text12 = new TextObject("{=qAbMagWq}sold produce at the market.");
        var effectedSkills12 = new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        };
        var social1 = DefaultCharacterAttributes.Social;
        var focusToAdd12 = FocusToAdd;
        var skillLevelToAdd12 = SkillLevelToAdd;
        var attributeLevelToAdd12 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition12 = RuralAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect12 = RuralAdolescenceHelperOnConsequence;
        CharacterCreationApplyFinalEffects onApply12 = null;
        var descriptionText12 = new TextObject("{=DIgsfYfz}You took your family's goods to the nearest town to sell your produce and buy supplies. It was hard work, but you enjoyed the hubbub of the marketplace.");
        creationCategory.AddCategoryOption(text12, effectedSkills12, social1, focusToAdd12, skillLevelToAdd12, attributeLevelToAdd12, optionCondition12, onSelect12, onApply12, descriptionText12);
        var text13 = new TextObject("{=nOfSqRnI}at the town watch's training ground.");
        var effectedSkills13 = new List<SkillObject>
        {
            DefaultSkills.Crossbow,
            DefaultSkills.Tactics
        };
        var control3 = DefaultCharacterAttributes.Control;
        var focusToAdd13 = FocusToAdd;
        var skillLevelToAdd13 = SkillLevelToAdd;
        var attributeLevelToAdd13 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition13 = UrbanAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect13 = UrbanAdolescenceWatcherOnConsequence;
        CharacterCreationApplyFinalEffects onApply13 = null;
        var descriptionText13 = new TextObject("{=qnqdEJOv}You watched the town's watch practice shooting and perfect their plans to defend the walls in case of a siege.");
        creationCategory.AddCategoryOption(text13, effectedSkills13, control3, focusToAdd13, skillLevelToAdd13, attributeLevelToAdd13, optionCondition13, onSelect13, onApply13, descriptionText13);
        var text14 = new TextObject("{=8a6dnLd2}with the alley gangs.");
        var effectedSkills14 = new List<SkillObject>
        {
            DefaultSkills.Roguery,
            DefaultSkills.OneHanded
        };
        var cunning = DefaultCharacterAttributes.Cunning;
        var focusToAdd14 = FocusToAdd;
        var skillLevelToAdd14 = SkillLevelToAdd;
        var attributeLevelToAdd14 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition14 = UrbanAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect14 = UrbanAdolescenceGangerOnConsequence;
        CharacterCreationApplyFinalEffects onApply14 = null;
        var descriptionText14 = new TextObject("{=1SUTcF0J}The gang leaders who kept watch over the slums of cities were always in need of poor youth to run messages and back them up in turf wars, while thrill-seeking merchants' sons and daughters sometimes slummed it in their company as well.");
        creationCategory.AddCategoryOption(text14, effectedSkills14, cunning, focusToAdd14, skillLevelToAdd14, attributeLevelToAdd14, optionCondition14, onSelect14, onApply14, descriptionText14);
        var text15 = new TextObject("{=7Hv984Sf}at docks and building sites.");
        var effectedSkills15 = new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Crafting
        };
        var vigor2 = DefaultCharacterAttributes.Vigor;
        var focusToAdd15 = FocusToAdd;
        var skillLevelToAdd15 = SkillLevelToAdd;
        var attributeLevelToAdd15 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition15 = UrbanAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect15 = UrbanAdolescenceDockerOnConsequence;
        CharacterCreationApplyFinalEffects onApply15 = null;
        var descriptionText15 = new TextObject("{=bhdkegZ4}All towns had their share of projects that were constantly in need of both skilled and unskilled labor. You learned how hoists and scaffolds were constructed, how planks and stones were hewn and fitted, and other skills.");
        creationCategory.AddCategoryOption(text15, effectedSkills15, vigor2, focusToAdd15, skillLevelToAdd15, attributeLevelToAdd15, optionCondition15, onSelect15, onApply15, descriptionText15);
        var text2 = new TextObject("{=kbcwb5TH}in the markets and caravanserais.");
        var effectedSkills2 = new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        };
        var social2 = DefaultCharacterAttributes.Social;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition2 = UrbanPoorAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect2 = UrbanAdolescenceMarketerOnConsequence;
        CharacterCreationApplyFinalEffects onApply2 = null;
        var descriptionText2 = new TextObject("{=lLJh7WAT}You worked in the marketplace, selling trinkets and drinks to busy shoppers.");
        creationCategory.AddCategoryOption(text2, effectedSkills2, social2, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, optionCondition2, onSelect2, onApply2, descriptionText2);
        var text3 = new TextObject("{=kbcwb5TH}in the markets and caravanserais.");
        var effectedSkills3 = new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Charm
        };
        var social3 = DefaultCharacterAttributes.Social;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition3 = UrbanRichAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect3 = UrbanAdolescenceMarketerOnConsequence;
        CharacterCreationApplyFinalEffects onApply3 = null;
        var descriptionText3 = new TextObject("{=rmMcwSn8}You helped your family handle their business affairs, going down to the marketplace to make purchases and oversee the arrival of caravans.");
        creationCategory.AddCategoryOption(text3, effectedSkills3, social3, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, optionCondition3, onSelect3, onApply3, descriptionText3);
        var text4 = new TextObject("{=mfRbx5KE}reading and studying.");
        var effectedSkills4 = new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Leadership
        };
        var intelligence2 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition4 = UrbanPoorAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect4 = UrbanAdolescenceTutorOnConsequence;
        CharacterCreationApplyFinalEffects onApply4 = null;
        var descriptionText4 = new TextObject("{=elQnygal}Your family scraped up the money for a rudimentary schooling and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates.");
        creationCategory.AddCategoryOption(text4, effectedSkills4, intelligence2, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, optionCondition4, onSelect4, onApply4, descriptionText4);
        var text5 = new TextObject("{=etG87fB7}with your tutor.");
        var effectedSkills5 = new List<SkillObject>
        {
            DefaultSkills.Engineering,
            DefaultSkills.Leadership
        };
        var intelligence3 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition5 = UrbanRichAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect5 = UrbanAdolescenceTutorOnConsequence;
        CharacterCreationApplyFinalEffects onApply5 = null;
        var descriptionText5 = new TextObject("{=hXl25avg}Your family arranged for a private tutor and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates.");
        creationCategory.AddCategoryOption(text5, effectedSkills5, intelligence3, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, optionCondition5, onSelect5, onApply5, descriptionText5);
        var text6 = new TextObject("{=FKpLEamz}caring for horses.");
        var effectedSkills6 = new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Steward
        };
        var endurance2 = DefaultCharacterAttributes.Endurance;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition6 = UrbanRichAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect6 = UrbanAdolescenceHorserOnConsequence;
        CharacterCreationApplyFinalEffects onApply6 = null;
        var descriptionText6 = new TextObject("{=Ghz90npw}Your family owned a few horses at the town stables and you took charge of their care. Many evenings you would take them out beyond the walls and gallup through the fields, racing other youth.");
        creationCategory.AddCategoryOption(text6, effectedSkills6, endurance2, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, optionCondition6, onSelect6, onApply6, descriptionText6);
        var text7 = new TextObject("{=vH7GtuuK}working at the stables.");
        var effectedSkills7 = new List<SkillObject>
        {
            DefaultSkills.Riding,
            DefaultSkills.Steward
        };
        var endurance3 = DefaultCharacterAttributes.Endurance;
        var focusToAdd7 = FocusToAdd;
        var skillLevelToAdd7 = SkillLevelToAdd;
        var attributeLevelToAdd7 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition7 = UrbanPoorAdolescenceOnCondition;
        CharacterCreationOnSelect onSelect7 = UrbanAdolescenceHorserOnConsequence;
        CharacterCreationApplyFinalEffects onApply7 = null;
        var descriptionText7 = new TextObject("{=csUq1RCC}You were employed as a hired hand at the town's stables. The overseers recognized that you had a knack for horses, and you were allowed to exercise them and sometimes even break in new steeds.");
        creationCategory.AddCategoryOption(text7, effectedSkills7, endurance3, focusToAdd7, skillLevelToAdd7, attributeLevelToAdd7, optionCondition7, onSelect7, onApply7, descriptionText7);
    }

    private void RuralAdolescenceHerderOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_streets" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "carry_bostaff_rogue1", isLeftHand: true);
    }

    private void RuralAdolescenceSmithyOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_militia" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "peasant_hammer_1_t1", isLeftHand: true);
    }

    private void RuralAdolescenceRepairmanOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_grit" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "carry_hammer", isLeftHand: true);
    }

    private void RuralAdolescenceGathererOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_basket_a", isLeftHand: true);
    }

    private void RuralAdolescenceHunterOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "composite_bow", isLeftHand: true);
    }

    private void RuralAdolescenceHelperOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers_2" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_fabric_c", isLeftHand: true);
    }

    private void UrbanAdolescenceWatcherOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_fox" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "", isLeftHand: true);
    }

    private void UrbanAdolescenceMarketerOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "", isLeftHand: true);
    }

    private void UrbanAdolescenceGangerOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "", isLeftHand: true);
    }

    private void UrbanAdolescenceDockerOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_basket_a", isLeftHand: true);
    }

    private void UrbanAdolescenceHorserOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_peddlers_2" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "_to_carry_bd_fabric_c", isLeftHand: true);
    }

    private void UrbanAdolescenceTutorOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_book" });
        RefreshPropsAndClothing(characterCreation, isChildhoodStage: false, "character_creation_notebook", isLeftHand: false);
    }

    private void AddYouthMenu(CharacterCreation characterCreation)
    {
        var youthMenu = new CharacterCreationMenu(new TextObject("{=ok8lSW6M}Youth"), YouthIntroductoryText, YouthOnInit);
        var creationCategory = youthMenu.AddMenuCategory();
        var text1 = new TextObject("{=TraWitAr}Trained with the Army");
        var effectedSkills1 = new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.Bow
        };
        var vigor1 = DefaultCharacterAttributes.Vigor;
        var focusToAdd1 = FocusToAdd;
        var skillLevelToAdd1 = SkillLevelToAdd;
        var attributeLevelToAdd1 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition1 = YouthCommanderOnCondition;
        CharacterCreationOnSelect onSelect1 = YouthCommanderOnConsequence;
        CharacterCreationApplyFinalEffects onApply1 = YouthCommanderOnApply;
        var descriptionText1 = new TextObject("{=TraArmTx}You spent your time at the training grounds of your home region learning the basics of weapon combat. Through this training you became proficient in Single Handed combat and Bowcraft");
        creationCategory.AddCategoryOption(text1, effectedSkills1, vigor1, focusToAdd1, skillLevelToAdd1, attributeLevelToAdd1, optionCondition1, onSelect1, onApply1, descriptionText1);
        var text2 = new TextObject("{=SpeTimMa}Spent time in the Markets");
        var effectedSkills2 = new List<SkillObject>
        {
            DefaultSkills.Charm,
            DefaultSkills.Roguery
        };
        var social1 = DefaultCharacterAttributes.Social;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition2 = YouthMerchantOnCondition;
        CharacterCreationOnSelect onSelect2 = YouthMerchantOnConsequence;
        CharacterCreationApplyFinalEffects onApply2 = YouthMerchantOnApply;
        var descriptionText2 = new TextObject("{=TimMarTx}You spent your time in the various markets across Westeros. Here you learned the arts of trade, from charming your patrons to slyly backstabbing your competition.");
        creationCategory.AddCategoryOption(text2, effectedSkills2, social1, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, optionCondition2, onSelect2, onApply2, descriptionText2);
        var text3 = new TextObject("{=TraAcWes}Traveled across Westeros");
        var effectedSkills3 = new List<SkillObject>
        {
            DefaultSkills.Crossbow,
            DefaultSkills.Scouting
        };
        var cunning1 = DefaultCharacterAttributes.Cunning;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition3 = YouthWanderlustOnCondition;
        CharacterCreationOnSelect onSelect3 = YouthWanderlustOnConsequence;
        CharacterCreationApplyFinalEffects onApply3 = YouthWanderlustOnApply;
        var descriptionText3 = new TextObject("{=TraWesTx}You spent your time traveling across the lands Westeros. You learned how to navigate across treacherous terrain, armed with a crossbow to protect yourself you became proficient as a traveler.");
        creationCategory.AddCategoryOption(text3, effectedSkills3, cunning1, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, optionCondition3, onSelect3, onApply3, descriptionText3);
        var text4 = new TextObject("{=HidWiBan}Hid out with Bandits");
        var effectedSkills4 = new List<SkillObject>
        {
            DefaultSkills.Leadership,
            DefaultSkills.Polearm
        };
        var control1 = DefaultCharacterAttributes.Control;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition4 = YouthRaiderOnCondition;
        CharacterCreationOnSelect onSelect4 = YouthRaiderOnConsequence;
        CharacterCreationApplyFinalEffects onApply4 = YouthRaiderOnApply;
        var descriptionText4 = new TextObject("{=HidBanTx}You spent time with the bandits of your home, raiding and attacking the various villages nearby. You learned how to organise large groups and conduct yourself as a leader, they also showed you how to use polearms to protect yourself.");
        creationCategory.AddCategoryOption(text4, effectedSkills4, control1, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, optionCondition4, onSelect4, onApply4, descriptionText4);
        var text5 = new TextObject("{=LeaToSmi}Learning how to Smith");
        var effectedSkills5 = new List<SkillObject>
        {
            DefaultSkills.Crafting,
            DefaultSkills.Athletics
        };
        var endurance1 = DefaultCharacterAttributes.Endurance;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition5 = YouthSmithOnCondition;
        CharacterCreationOnSelect onSelect5 = YouthSmithOnConsequence;
        CharacterCreationApplyFinalEffects onApply5 = YouthSmithOnApply;
        var descriptionText5 = new TextObject("{=LeaSmiTx}Your time was spent working in the forges. Because of this you understand how metals work together and the making of various weapons. Your body has grown strong because this work so you can endure in a fight for much longer.");
        creationCategory.AddCategoryOption(text5, effectedSkills5, endurance1, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, optionCondition5, onSelect5, onApply5, descriptionText5);
        var text6 = new TextObject("{=}Stood Guard with the Militia");
        var effectedSkills6 = new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Engineering
        };
        var intelligence1 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition6 = YouthMilitiaOnCondition;
        CharacterCreationOnSelect onSelect6 = YouthMilitiaOnConsequence;
        CharacterCreationApplyFinalEffects onApply6 = YouthMilitiaOnApply;
        var descriptionText6 = new TextObject("{=}");
        creationCategory.AddCategoryOption(text6, effectedSkills6, intelligence1, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, optionCondition6, onSelect6, onApply6, descriptionText6);
        characterCreation.AddNewMenu(youthMenu);
    }

    private void YouthCommanderOnApply(CharacterCreation characterCreation)
    {
    }

    private void YouthMerchantOnApply(CharacterCreation characterCreation)
    {
    }

    private void YouthWanderlustOnApply(CharacterCreation characterCreation)
    {
    }

    private void YouthRaiderOnApply(CharacterCreation characterCreation)
    {
    }

    private void YouthSmithOnApply(CharacterCreation characterCreation)
    {
    }

    private void YouthMilitiaOnApply(CharacterCreation characterCreation)
    {
    }

    private void YouthCommanderOnConsequence(CharacterCreation characterCreation)
    {
        base.SelectedTitleType = 1;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    private void YouthMerchantOnConsequence(CharacterCreation characterCreation)
    {
        base.SelectedTitleType = 2;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    private void YouthWanderlustOnConsequence(CharacterCreation characterCreation)
    {
        base.SelectedTitleType = 3;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    private void YouthRaiderOnConsequence(CharacterCreation characterCreation)
    {
        base.SelectedTitleType = 4;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    private void YouthSmithOnConsequence(CharacterCreation characterCreation)
    {
        base.SelectedTitleType = 5;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    private void YouthMilitiaOnConsequence(CharacterCreation characterCreation)
    {
        base.SelectedTitleType = 6;
        RefreshPlayerAppearance(characterCreation);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
    }

    private bool YouthCommanderOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = ((!(stringId == "norvos")) ? 1 : 0);
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
                num = 0;
                break;
        }

        return num == 0;
    }

    private bool YouthMerchantOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = ((!(stringId == "norvos")) ? 1 : 0);
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
                num = 0;
                break;
        }

        return num == 0;
    }

    private bool YouthWanderlustOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = ((!(stringId == "norvos")) ? 1 : 0);
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
                num = 0;
                break;
        }

        return num == 0;
    }

    private bool YouthRaiderOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = ((!(stringId == "norvos")) ? 1 : 0);
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
                num = 0;
                break;
        }

        return num == 0;
    }

    private bool YouthSmithOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = ((!(stringId == "norvos")) ? 1 : 0);
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
                num = 0;
                break;
        }

        return num == 0;
    }

    private bool YouthMilitiaOnCondition()
    {
        var stringId = GetSelectedCulture().StringId;
        int num;
        switch (stringId)
        {
            default:
                num = ((!(stringId == "norvos")) ? 1 : 0);
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
                num = 0;
                break;
        }

        return num == 0;
    }

    private void AddAdulthoodMenu(CharacterCreation characterCreation)
    {
        MBTextManager.SetTextVariable("EXP_VALUE", SkillLevelToAdd);
        var adulthoodMenu = new CharacterCreationMenu(new TextObject("{=MafIe9yI}Young Adulthood"), new TextObject("{=4WYY0X59}Before you set out for a life of adventure, your biggest achievement was..."), AccomplishmentOnInit);
        characterCreation.AddNewMenu(adulthoodMenu);
        var creationCategory = adulthoodMenu.AddMenuCategory();
        var text1 = new TextObject("{=8bwpVpgy}you defeated an enemy in battle.");
        var effectedSkills1 = new List<SkillObject>
        {
            DefaultSkills.OneHanded,
            DefaultSkills.TwoHanded
        };
        var vigor = DefaultCharacterAttributes.Vigor;
        var focusToAdd1 = FocusToAdd;
        var skillLevelToAdd1 = SkillLevelToAdd;
        var attributeLevelToAdd1 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect1 = AccomplishmentDefeatedEnemyOnConsequence;
        CharacterCreationApplyFinalEffects onApply1 = AccomplishmentDefeatedEnemyOnApply;
        var descriptionText1 = new TextObject("{=1IEroJKs}Not everyone who musters for the levy marches to war, and not everyone who goes on campaign sees action. You did both, and you also took down an enemy warrior in direct one-to-one combat, in the full view of your comrades.");
        creationCategory.AddCategoryOption(text1, effectedSkills1, vigor, focusToAdd1, skillLevelToAdd1, attributeLevelToAdd1, null, onSelect1, onApply1, descriptionText1, new List<TraitObject> { DefaultTraits.Valor }, 1, 20);
        var text5 = new TextObject("{=mP3uFbcq}you led a successful manhunt.");
        var effectedSkills5 = new List<SkillObject>
        {
            DefaultSkills.Tactics,
            DefaultSkills.Leadership
        };
        var cunning1 = DefaultCharacterAttributes.Cunning;
        var focusToAdd5 = FocusToAdd;
        var skillLevelToAdd5 = SkillLevelToAdd;
        var attributeLevelToAdd5 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition1 = AccomplishmentPosseOnConditions;
        CharacterCreationOnSelect onSelect5 = AccomplishmentExpeditionOnConsequence;
        CharacterCreationApplyFinalEffects onApply5 = AccomplishmentExpeditionOnApply;
        var descriptionText5 = new TextObject("{=4f5xwzX0}When your community needed to organize a posse to pursue horse thieves, you were the obvious choice. You hunted down the raiders, surrounded them and forced their surrender, and took back your stolen property.");
        creationCategory.AddCategoryOption(text5, effectedSkills5, cunning1, focusToAdd5, skillLevelToAdd5, attributeLevelToAdd5, optionCondition1, onSelect5, onApply5, descriptionText5, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text6 = new TextObject("{=wfbtS71d}you led a caravan.");
        var effectedSkills6 = new List<SkillObject>
        {
            DefaultSkills.Tactics,
            DefaultSkills.Leadership
        };
        var cunning2 = DefaultCharacterAttributes.Cunning;
        var focusToAdd6 = FocusToAdd;
        var skillLevelToAdd6 = SkillLevelToAdd;
        var attributeLevelToAdd6 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition3 = AccomplishmentMerchantOnCondition;
        CharacterCreationOnSelect onSelect6 = AccomplishmentMerchantOnConsequence;
        CharacterCreationApplyFinalEffects onApply6 = AccomplishmentExpeditionOnApply;
        var descriptionText6 = new TextObject("{=joRHKCkm}Your family needed someone trustworthy to take a caravan to a neighboring town. You organized supplies, ensured a constant watch to keep away bandits, and brought it safely to its destination.");
        creationCategory.AddCategoryOption(text6, effectedSkills6, cunning2, focusToAdd6, skillLevelToAdd6, attributeLevelToAdd6, optionCondition3, onSelect6, onApply6, descriptionText6, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text7 = new TextObject("{=x1HTX5hq}you saved your village from a flood.");
        var effectedSkills7 = new List<SkillObject>
        {
            DefaultSkills.Tactics,
            DefaultSkills.Leadership
        };
        var cunning3 = DefaultCharacterAttributes.Cunning;
        var focusToAdd7 = FocusToAdd;
        var skillLevelToAdd7 = SkillLevelToAdd;
        var attributeLevelToAdd7 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition4 = AccomplishmentSavedVillageOnCondition;
        CharacterCreationOnSelect onSelect7 = AccomplishmentSavedVillageOnConsequence;
        CharacterCreationApplyFinalEffects onApply7 = AccomplishmentExpeditionOnApply;
        var descriptionText7 = new TextObject("{=bWlmGDf3}When a sudden storm caused the local stream to rise suddenly, your neighbors needed quick-thinking leadership. You provided it, directing them to build levees to save their homes.");
        creationCategory.AddCategoryOption(text7, effectedSkills7, cunning3, focusToAdd7, skillLevelToAdd7, attributeLevelToAdd7, optionCondition4, onSelect7, onApply7, descriptionText7, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text8 = new TextObject("{=s8PNllPN}you saved your city quarter from a fire.");
        var effectedSkills8 = new List<SkillObject>
        {
            DefaultSkills.Tactics,
            DefaultSkills.Leadership
        };
        var cunning4 = DefaultCharacterAttributes.Cunning;
        var focusToAdd8 = FocusToAdd;
        var skillLevelToAdd8 = SkillLevelToAdd;
        var attributeLevelToAdd8 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition5 = AccomplishmentSavedStreetOnCondition;
        CharacterCreationOnSelect onSelect8 = AccomplishmentSavedStreetOnConsequence;
        CharacterCreationApplyFinalEffects onApply8 = AccomplishmentExpeditionOnApply;
        var descriptionText8 = new TextObject("{=ZAGR6PYc}When a sudden blaze broke out in a back alley, your neighbors needed quick-thinking leadership and you provided it. You organized a bucket line to the nearest well, putting the fire out before any homes were lost.");
        creationCategory.AddCategoryOption(text8, effectedSkills8, cunning4, focusToAdd8, skillLevelToAdd8, attributeLevelToAdd8, optionCondition5, onSelect8, onApply8, descriptionText8, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text9 = new TextObject("{=xORjDTal}you invested some money in a workshop.");
        var effectedSkills9 = new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Crafting
        };
        var intelligence1 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd9 = FocusToAdd;
        var skillLevelToAdd9 = SkillLevelToAdd;
        var attributeLevelToAdd9 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition6 = AccomplishmentUrbanOnCondition;
        CharacterCreationOnSelect onSelect9 = AccomplishmentWorkshopOnConsequence;
        CharacterCreationApplyFinalEffects onApply9 = AccomplishmentWorkshopOnApply;
        var descriptionText9 = new TextObject("{=PyVqDLBu}Your parents didn't give you much money, but they did leave just enough for you to secure a loan against a larger amount to build a small workshop. You paid back what you borrowed, and sold your enterprise for a profit.");
        creationCategory.AddCategoryOption(text9, effectedSkills9, intelligence1, focusToAdd9, skillLevelToAdd9, attributeLevelToAdd9, optionCondition6, onSelect9, onApply9, descriptionText9, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text10 = new TextObject("{=xKXcqRJI}you invested some money in land.");
        var effectedSkills10 = new List<SkillObject>
        {
            DefaultSkills.Trade,
            DefaultSkills.Crafting
        };
        var intelligence2 = DefaultCharacterAttributes.Intelligence;
        var focusToAdd10 = FocusToAdd;
        var skillLevelToAdd10 = SkillLevelToAdd;
        var attributeLevelToAdd10 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition7 = AccomplishmentRuralOnCondition;
        CharacterCreationOnSelect onSelect10 = AccomplishmentWorkshopOnConsequence;
        CharacterCreationApplyFinalEffects onApply10 = AccomplishmentWorkshopOnApply;
        var descriptionText10 = new TextObject("{=cbF9jdQo}Your parents didn't give you much money, but they did leave just enough for you to purchase a plot of unused land at the edge of the village. You cleared away rocks and dug an irrigation ditch, raised a few seasons of crops, than sold it for a considerable profit.");
        creationCategory.AddCategoryOption(text10, effectedSkills10, intelligence2, focusToAdd10, skillLevelToAdd10, attributeLevelToAdd10, optionCondition7, onSelect10, onApply10, descriptionText10, new List<TraitObject> { DefaultTraits.Calculating }, 1, 10);
        var text11 = new TextObject("{=TbNRtUjb}you hunted a dangerous animal.");
        var effectedSkills11 = new List<SkillObject>
        {
            DefaultSkills.Polearm,
            DefaultSkills.Crossbow
        };
        var control1 = DefaultCharacterAttributes.Control;
        var focusToAdd11 = FocusToAdd;
        var skillLevelToAdd11 = SkillLevelToAdd;
        var attributeLevelToAdd11 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition8 = AccomplishmentRuralOnCondition;
        CharacterCreationOnSelect onSelect11 = AccomplishmentSiegeHunterOnConsequence;
        CharacterCreationApplyFinalEffects onApply11 = AccomplishmentSiegeHunterOnApply;
        var descriptionText11 = new TextObject("{=I3PcdaaL}Wolves, bears are a constant menace to the flocks of northern Calradia, while hyenas and leopards trouble the south. You went with a group of your fellow villagers and fired the missile that brought down the beast.");
        creationCategory.AddCategoryOption(text11, effectedSkills11, control1, focusToAdd11, skillLevelToAdd11, attributeLevelToAdd11, optionCondition8, onSelect11, onApply11, descriptionText11, null, 0, 5);
        var text12 = new TextObject("{=WbHfGCbd}you survived a siege.");
        var effectedSkills12 = new List<SkillObject>
        {
            DefaultSkills.Bow,
            DefaultSkills.Crossbow
        };
        var control2 = DefaultCharacterAttributes.Control;
        var focusToAdd12 = FocusToAdd;
        var skillLevelToAdd12 = SkillLevelToAdd;
        var attributeLevelToAdd12 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition9 = AccomplishmentUrbanOnCondition;
        CharacterCreationOnSelect onSelect12 = AccomplishmentSiegeHunterOnConsequence;
        CharacterCreationApplyFinalEffects onApply12 = AccomplishmentSiegeHunterOnApply;
        var descriptionText12 = new TextObject("{=FhZPjhli}Your hometown was briefly placed under siege, and you were called to defend the walls. Everyone did their part to repulse the enemy assault, and everyone is justly proud of what they endured.");
        creationCategory.AddCategoryOption(text12, effectedSkills12, control2, focusToAdd12, skillLevelToAdd12, attributeLevelToAdd12, optionCondition9, onSelect12, onApply12, descriptionText12, null, 0, 5);
        var text2 = new TextObject("{=kNXet6Um}you had a famous escapade in town.");
        var effectedSkills2 = new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Roguery
        };
        var endurance1 = DefaultCharacterAttributes.Endurance;
        var focusToAdd2 = FocusToAdd;
        var skillLevelToAdd2 = SkillLevelToAdd;
        var attributeLevelToAdd2 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition10 = AccomplishmentRuralOnCondition;
        CharacterCreationOnSelect onSelect2 = AccomplishmentEscapadeOnConsequence;
        CharacterCreationApplyFinalEffects onApply2 = AccomplishmentEscapadeOnApply;
        var descriptionText2 = new TextObject("{=DjeAJtix}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, on one of your trips into town you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.");
        creationCategory.AddCategoryOption(text2, effectedSkills2, endurance1, focusToAdd2, skillLevelToAdd2, attributeLevelToAdd2, optionCondition10, onSelect2, onApply2, descriptionText2, new List<TraitObject> { DefaultTraits.Valor }, 1, 5);
        var text3 = new TextObject("{=qlOuiKXj}you had a famous escapade.");
        var effectedSkills3 = new List<SkillObject>
        {
            DefaultSkills.Athletics,
            DefaultSkills.Roguery
        };
        var endurance2 = DefaultCharacterAttributes.Endurance;
        var focusToAdd3 = FocusToAdd;
        var skillLevelToAdd3 = SkillLevelToAdd;
        var attributeLevelToAdd3 = AttributeLevelToAdd;
        CharacterCreationOnCondition optionCondition2 = AccomplishmentUrbanOnCondition;
        CharacterCreationOnSelect onSelect3 = AccomplishmentEscapadeOnConsequence;
        CharacterCreationApplyFinalEffects onApply3 = AccomplishmentEscapadeOnApply;
        var descriptionText3 = new TextObject("{=lD5Ob3R4}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.");
        creationCategory.AddCategoryOption(text3, effectedSkills3, endurance2, focusToAdd3, skillLevelToAdd3, attributeLevelToAdd3, optionCondition2, onSelect3, onApply3, descriptionText3, new List<TraitObject> { DefaultTraits.Valor }, 1, 5);
        var text4 = new TextObject("{=Yqm0Dics}you treated people well.");
        var effectedSkills4 = new List<SkillObject>
        {
            DefaultSkills.Charm,
            DefaultSkills.Steward
        };
        var social = DefaultCharacterAttributes.Social;
        var focusToAdd4 = FocusToAdd;
        var skillLevelToAdd4 = SkillLevelToAdd;
        var attributeLevelToAdd4 = AttributeLevelToAdd;
        CharacterCreationOnSelect onSelect4 = AccomplishmentTreaterOnConsequence;
        CharacterCreationApplyFinalEffects onApply4 = AccomplishmentTreaterOnApply;
        var descriptionText4 = new TextObject("{=dDmcqTzb}Yours wasn't the kind of reputation that local legends are made of, but it was the kind that wins you respect among those around you. You were consistently fair and honest in your business dealings and helpful to those in trouble. In doing so, you got a sense of what made people tick.");
        creationCategory.AddCategoryOption(text4, effectedSkills4, social, focusToAdd4, skillLevelToAdd4, attributeLevelToAdd4, null, onSelect4, onApply4, descriptionText4, new List<TraitObject>
        {
            DefaultTraits.Mercy,
            DefaultTraits.Generosity,
            DefaultTraits.Honor
        }, 1, 5);
    }

    private void AccomplishmentDefeatedEnemyOnApply(CharacterCreation characterCreation)
    {
    }

    private void AccomplishmentExpeditionOnApply(CharacterCreation characterCreation)
    {
    }

    private bool AccomplishmentRuralOnCondition()
    {
        return RuralType();
    }

    private bool AccomplishmentMerchantOnCondition()
    {
        return FamilyOccupationType == OccupationTypes.Merchant;
    }

    private bool AccomplishmentPosseOnConditions()
    {
        return FamilyOccupationType == OccupationTypes.Retainer || FamilyOccupationType == OccupationTypes.Herder || FamilyOccupationType == OccupationTypes.Mercenary;
    }

    private bool AccomplishmentSavedVillageOnCondition()
    {
        return RuralType() && FamilyOccupationType != OccupationTypes.Retainer && FamilyOccupationType != OccupationTypes.Herder;
    }

    private bool AccomplishmentSavedStreetOnCondition()
    {
        return !RuralType() && FamilyOccupationType != OccupationTypes.Merchant && FamilyOccupationType != OccupationTypes.Mercenary;
    }

    private bool AccomplishmentUrbanOnCondition()
    {
        return !RuralType();
    }

    private void AccomplishmentWorkshopOnApply(CharacterCreation characterCreation)
    {
    }

    private void AccomplishmentSiegeHunterOnApply(CharacterCreation characterCreation)
    {
    }

    private void AccomplishmentEscapadeOnApply(CharacterCreation characterCreation)
    {
    }

    private void AccomplishmentTreaterOnApply(CharacterCreation characterCreation)
    {
    }

    private void AccomplishmentDefeatedEnemyOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_athlete" });
    }

    private void AccomplishmentExpeditionOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_gracious" });
    }

    private void AccomplishmentMerchantOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_ready" });
    }

    private void AccomplishmentSavedVillageOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_vibrant" });
    }

    private void AccomplishmentSavedStreetOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_vibrant" });
    }

    private void AccomplishmentWorkshopOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_decisive" });
    }

    private void AccomplishmentSiegeHunterOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_tough" });
    }

    private void AccomplishmentEscapadeOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_clever" });
    }

    private void AccomplishmentTreaterOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_manners" });
    }

    private void AddAgeSelectionMenu(CharacterCreation characterCreation)
    {
        MBTextManager.SetTextVariable("EXP_VALUE", SkillLevelToAdd);
        var ageSelectionMenu = new CharacterCreationMenu(new TextObject("{=HDFEAYDk}Starting Age"), new TextObject("{=VlOGrGSn}Your character started off on the adventuring path at the age of..."), StartingAgeOnInit);
        var creationCategory = ageSelectionMenu.AddMenuCategory();
        creationCategory.AddCategoryOption(new TextObject("{=!}20"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeYoungOnConsequence, StartingAgeYoungOnApply, new TextObject("{=2k7adlh7}While lacking experience a bit, you are full with youthful energy, you are fully eager, for the long years of adventuring ahead."), null, 0, 0, 0, 2, 1);
        creationCategory.AddCategoryOption(new TextObject("{=!}30"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeAdultOnConsequence, StartingAgeAdultOnApply, new TextObject("{=NUlVFRtK}You are at your prime, You still have some youthful energy but also have a substantial amount of experience under your belt. "), null, 0, 0, 0, 4, 2);
        creationCategory.AddCategoryOption(new TextObject("{=!}40"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeMiddleAgedOnConsequence, StartingAgeMiddleAgedOnApply, new TextObject("{=5MxTYApM}This is the right age for starting off, you have years of experience, and you are old enough for people to respect you and gather under your banner."), null, 0, 0, 0, 6, 3);
        creationCategory.AddCategoryOption(new TextObject("{=!}50"), new List<SkillObject>(), null, 0, 0, 0, null, StartingAgeElderlyOnConsequence, StartingAgeElderlyOnApply, new TextObject("{=ePD5Afvy}While you are past your prime, there is still enough time to go on that last big adventure for you. And you have all the experience you need to overcome anything!"), null, 0, 0, 0, 8, 4);
        characterCreation.AddNewMenu(ageSelectionMenu);
    }

    private void ParentsOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = false;
        characterCreation.HasSecondaryCharacter = false;
        ClearMountEntity(characterCreation);
        characterCreation.ClearFaceGenPrefab();
        if (base.PlayerBodyProperties != CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment))
        {
            base.PlayerBodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment);
            var motherBodyProperties = base.PlayerBodyProperties;
            var fatherBodyProperties = base.PlayerBodyProperties;
            FaceGen.GenerateParentKey(base.PlayerBodyProperties, CharacterObject.PlayerCharacter.Race, ref motherBodyProperties, ref fatherBodyProperties);
            motherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.3f, 0.2f), motherBodyProperties.StaticProperties);
            fatherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.5f, 0.5f), fatherBodyProperties.StaticProperties);
            base.MotherFacegenCharacter = new FaceGenChar(motherBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), isFemale: true, "anim_mother_1");
            base.FatherFacegenCharacter = new FaceGenChar(fatherBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), isFemale: false, "anim_father_1");
        }

        characterCreation.ChangeFaceGenChars(new List<FaceGenChar> { base.MotherFacegenCharacter, base.FatherFacegenCharacter });
        ChangeParentsOutfit(characterCreation);
        ChangeParentsAnimation(characterCreation);
    }

    private void ChangeParentsOutfit(CharacterCreation characterCreation, string fatherItemId = "", string motherItemId = "", bool isLeftHandItemForFather = true, bool isLeftHandItemForMother = true)
    {
        characterCreation.ClearFaceGenPrefab();
        var equipmentList = new List<Equipment>();
        var equipment1 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("mother_char_creation_" + base.SelectedParentType + "_" + GetSelectedCulture().StringId)?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
        var equipment2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("father_char_creation_" + base.SelectedParentType + "_" + GetSelectedCulture().StringId)?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
        if (motherItemId != "")
        {
            var itemObject2 = Game.Current.ObjectManager.GetObject<ItemObject>(motherItemId);
            if (itemObject2 != null)
            {
                equipment1.AddEquipmentToSlotWithoutAgent((!isLeftHandItemForMother) ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(itemObject2));
            }
            else
            {
                var baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCreation.FaceGenChars[0].Race);
                characterCreation.ChangeCharacterPrefab(motherItemId, isLeftHandItemForMother ? baseMonsterFromRace.MainHandItemBoneIndex : baseMonsterFromRace.OffHandItemBoneIndex);
            }
        }

        if (fatherItemId != "")
        {
            var itemObject = Game.Current.ObjectManager.GetObject<ItemObject>(fatherItemId);
            if (itemObject != null)
            {
                equipment2.AddEquipmentToSlotWithoutAgent((!isLeftHandItemForFather) ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(itemObject));
            }
        }

        equipmentList.Add(equipment1);
        equipmentList.Add(equipment2);
        characterCreation.ChangeCharactersEquipment(equipmentList);
    }

    private void ChangeParentsAnimation(CharacterCreation characterCreation)
    {
        var actionList = new List<string>
        {
            "anim_mother_" + base.SelectedParentType,
            "anim_father_" + base.SelectedParentType
        };
        characterCreation.ChangeCharsAnimation(actionList);
    }

    private void SetParentAndOccupationType(CharacterCreation characterCreation, int parentType, OccupationTypes occupationType, string fatherItemId = "", string motherItemId = "", bool isLeftHandItemForFather = true, bool isLeftHandItemForMother = true)
    {
        base.SelectedParentType = parentType;
        FamilyOccupationType = occupationType;
        characterCreation.ChangeFaceGenChars(new List<FaceGenChar> { base.MotherFacegenCharacter, base.FatherFacegenCharacter });
        ChangeParentsAnimation(characterCreation);
        ChangeParentsOutfit(characterCreation, fatherItemId, motherItemId, isLeftHandItemForFather, isLeftHandItemForMother);
    }

    private void FreeFolkParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void FreeFolkParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void FreeFolkParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void FreeFolkParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void FreeFolkParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void FreeFolkParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void NightsWatchParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void NightsWatchParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void NightsWatchParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void NightsWatchParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void NightsWatchParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void NightsWatchParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void BattanianParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void BattanianParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void BattanianParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void BattanianParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void BattanianParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void BattanianParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void BoltonParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void BoltonParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void BoltonParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void BoltonParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void BoltonParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void BoltonParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void SturgiaParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void SturgiaParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void SturgiaParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void SturgiaParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void SturgiaParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void SturgiaParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void ValeParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void ValeParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void ValeParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void ValeParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void ValeParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void ValeParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void RiverlandsParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void RiverlandsParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void RiverlandsParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void RiverlandsParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void RiverlandsParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void RiverlandsParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void VlandiaParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void VlandiaParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void VlandiaParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void VlandiaParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void VlandiaParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void VlandiaParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void DragonstoneParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void DragonstoneParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void DragonstoneParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void DragonstoneParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void DragonstoneParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void DragonstoneParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void StormlandsParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void StormlandsParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void StormlandsParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void StormlandsParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void StormlandsParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void StormlandsParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void ReachParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void ReachParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void ReachParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void ReachParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void ReachParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void ReachParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void CrownlandsParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void CrownlandsParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void CrownlandsParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void CrownlandsParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void CrownlandsParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void CrownlandsParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void AseraiParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void AseraiParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void AseraiParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void AseraiParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void AseraiParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void AseraiParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void EmpireParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void EmpireParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void EmpireParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void EmpireParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void EmpireParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void EmpireParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void GhiscariParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void GhiscariParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void GhiscariParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void GhiscariParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void GhiscariParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void GhiscariParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void KhuzaitParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void KhuzaitParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void KhuzaitParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void KhuzaitParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void KhuzaitParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void KhuzaitParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void SarnorParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void SarnorParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void SarnorParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void SarnorParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void SarnorParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void SarnorParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }

    private void NorvosParentsOption1OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 1, OccupationTypes.Retainer);
    }

    private void NorvosParentsOption2OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 2, OccupationTypes.Merchant);
    }

    private void NorvosParentsOption3OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 3, OccupationTypes.Mercenary);
    }

    private void NorvosParentsOption4OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 4, OccupationTypes.Farmer);
    }

    private void NorvosParentsOption5OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 5, OccupationTypes.Bard);
    }

    private void NorvosParentsOption6OnConsequence(CharacterCreation characterCreation)
    {
        SetParentAndOccupationType(characterCreation, 6, OccupationTypes.Vagabond);
    }


    private bool FreefolkParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "freefolk";
    }

    private bool NightsWatchParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "nightswatch";
    }

    private bool BattanianParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "battania";
    }

    private bool BoltonParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "bolton";
    }

    private bool SturgiaParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "sturgia";
    }

    private bool ValeParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "vale";
    }

    private bool RiverlandsParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "river";
    }

    private bool VlandiaParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "vlandia";
    }

    private bool DragonstoneParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "dragonstone";
    }

    private bool StormlandsParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "stormlands";
    }

    private bool ReachParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "reach";
    }

    private bool CrownlandsParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "crownlands";
    }

    private bool AseraiParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "aserai";
    }

    private bool EmpireParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "empire";
    }

    private bool GhiscariParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "ghiscari";
    }

    private bool KhuzaitParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "khuzait";
    }

    private bool SarnorParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "sarnor";
    }

    private bool NorvosParentsOnCondition()
    {
        return GetSelectedCulture().StringId == "norvos";
    }

    private void FinalizeParents(CharacterCreation characterCreation) => FinalizeParents();

    private void FinalizeParents()
    {
        var character1 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_mother");
        var character2 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_father");
        character1.HeroObject.ModifyPlayersFamilyAppearance(base.MotherFacegenCharacter.BodyProperties.StaticProperties);
        character2.HeroObject.ModifyPlayersFamilyAppearance(base.FatherFacegenCharacter.BodyProperties.StaticProperties);
        character1.HeroObject.Weight = base.MotherFacegenCharacter.BodyProperties.Weight;
        character1.HeroObject.Build = base.MotherFacegenCharacter.BodyProperties.Build;
        character2.HeroObject.Weight = base.FatherFacegenCharacter.BodyProperties.Weight;
        character2.HeroObject.Build = base.FatherFacegenCharacter.BodyProperties.Build;
        EquipmentHelper.AssignHeroEquipmentFromEquipment(character1.HeroObject, base.MotherFacegenCharacter.Equipment);
        EquipmentHelper.AssignHeroEquipmentFromEquipment(character2.HeroObject, base.FatherFacegenCharacter.Equipment);
        character1.Culture = Hero.MainHero.Culture;
        character2.Culture = Hero.MainHero.Culture;
        StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter);
        var text1 = GameTexts.FindText("str_player_father_name", Hero.MainHero.Culture.StringId);
        character2.HeroObject.SetName(text1, text1);
        var parent1 = new TextObject("{=XmvaRfLM}{PLAYER_FATHER.NAME} was the father of {PLAYER.LINK}. He was slain when raiders attacked the inn at which his family was staying.");
        StringHelpers.SetCharacterProperties("PLAYER_FATHER", character2, parent1);
        character2.HeroObject.EncyclopediaText = parent1;
        var text2 = GameTexts.FindText("str_player_mother_name", Hero.MainHero.Culture.StringId);
        character1.HeroObject.SetName(text2, text2);
        var parent2 = new TextObject("{=hrhvEWP8}{PLAYER_MOTHER.NAME} was the mother of {PLAYER.LINK}. She was slain when raiders attacked the inn at which her family was staying.");
        StringHelpers.SetCharacterProperties("PLAYER_MOTHER", character1, parent2);
        character1.HeroObject.EncyclopediaText = parent2;
        character1.HeroObject.UpdateHomeSettlement();
        character2.HeroObject.UpdateHomeSettlement();
        character1.HeroObject.HasMet = true;
        character2.HeroObject.HasMet = true;
    }

    private static List<FaceGenChar> ChangePlayerFaceWithAge(float age, string actionName = "act_childhood_schooled")
    {
        var faceGenCharList = new List<FaceGenChar>();
        var originalBodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment);
        originalBodyProperties = FaceGen.GetBodyPropertiesWithAge(ref originalBodyProperties, age);
        faceGenCharList.Add(new FaceGenChar(originalBodyProperties, CharacterObject.PlayerCharacter.Race, new Equipment(), CharacterObject.PlayerCharacter.IsFemale, actionName));
        return faceGenCharList;
    }

    private Equipment ChangePlayerOutfit(CharacterCreation characterCreation, string outfit)
    {
        var equipmentList = new List<Equipment>();
        var equipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(outfit)?.DefaultEquipment ?? Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_default").DefaultEquipment;
        equipmentList.Add(equipment);
        characterCreation.ChangeCharactersEquipment(equipmentList);
        return equipment;
    }

    private static void ChangePlayerMount(CharacterCreation characterCreation, Hero hero)
    {
        if (hero.CharacterObject.HasMount())
        {
            var newMount = new FaceGenMount(MountCreationKey.GetRandomMountKey(hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, hero.CharacterObject.GetMountKeySeed()), hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, hero.CharacterObject.Equipment[EquipmentIndex.HorseHarness].Item, "act_horse_stand_1");
            characterCreation.SetFaceGenMount(newMount);
        }
    }

    private static void ClearMountEntity(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenMounts();
    }

    private void ChildhoodOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(ChildhoodAge));
        var outfit = "player_char_creation_childhood_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType + (Hero.MainHero.IsFemale ? "_f" : "_m");
        ChangePlayerOutfit(characterCreation, outfit);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        ClearMountEntity(characterCreation);
    }

    private void EducationOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        var textObject1 = new TextObject("{=WYvnWcXQ}Like all village children you helped out in the fields. You also...");
        var textObject2 = new TextObject("{=DsCkf6Pb}Growing up, you spent most of your time...");
        EducationIntroductoryText.SetTextVariable("EDUCATION_INTRO", RuralType() ? textObject1 : textObject2);
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(EducationAge));
        var outfit = "player_char_creation_education_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType + (Hero.MainHero.IsFemale ? "_f" : "_m");
        ChangePlayerOutfit(characterCreation, outfit);
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        ClearMountEntity(characterCreation);
    }

    private bool RuralType()
    {
        return FamilyOccupationType == OccupationTypes.Retainer || FamilyOccupationType == OccupationTypes.Farmer || FamilyOccupationType == OccupationTypes.Hunter || FamilyOccupationType == OccupationTypes.Bard || FamilyOccupationType == OccupationTypes.Herder || FamilyOccupationType == OccupationTypes.Vagabond || FamilyOccupationType == OccupationTypes.Healer || FamilyOccupationType == OccupationTypes.Artisan;
    }

    private bool RichParents()
    {
        return FamilyOccupationType == OccupationTypes.Retainer || FamilyOccupationType == OccupationTypes.Merchant;
    }

    private bool RuralAdolescenceOnCondition()
    {
        return RuralType();
    }

    private bool UrbanAdolescenceOnCondition()
    {
        return !RuralType();
    }

    private bool UrbanRichAdolescenceOnCondition()
    {
        return !RuralType() && RichParents();
    }

    private bool UrbanPoorAdolescenceOnCondition()
    {
        return !RuralType() && !RichParents();
    }

    private void RefreshPropsAndClothing(CharacterCreation characterCreation, bool isChildhoodStage, string itemId, bool isLeftHand, string secondItemId = "")
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ClearCharactersEquipment();
        var str = (isChildhoodStage ? ("player_char_creation_childhood_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType) : ("player_char_creation_education_age_" + GetSelectedCulture().StringId + "_" + base.SelectedParentType));
        var outfit = str + (Hero.MainHero.IsFemale ? "_f" : "_m");
        var equipment = ChangePlayerOutfit(characterCreation, outfit).Clone();
        if (Game.Current.ObjectManager.GetObject<ItemObject>(itemId) != null)
        {
            var itemObject1 = Game.Current.ObjectManager.GetObject<ItemObject>(itemId);
            equipment.AddEquipmentToSlotWithoutAgent((!isLeftHand) ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(itemObject1));
            if (secondItemId != "")
            {
                var itemObject2 = Game.Current.ObjectManager.GetObject<ItemObject>(secondItemId);
                equipment.AddEquipmentToSlotWithoutAgent(isLeftHand ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot, new EquipmentElement(itemObject2));
            }
        }
        else
        {
            var baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCreation.FaceGenChars[0].Race);
            characterCreation.ChangeCharacterPrefab(itemId, isLeftHand ? baseMonsterFromRace.MainHandItemBoneIndex : baseMonsterFromRace.OffHandItemBoneIndex);
        }

        characterCreation.ChangeCharactersEquipment(new List<Equipment> { equipment });
    }

    private void YouthOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        var textObject1 = new TextObject("{=F7OO5SAa}As a youngster growing up in Calradia, war was never too far away. You...");
        var textObject2 = new TextObject("{=5kbeAC7k}In wartorn Calradia, especially in frontier or tribal areas, some women as well as men learn to fight from an early age. You...");
        YouthIntroductoryText.SetTextVariable("YOUTH_INTRO", CharacterObject.PlayerCharacter.IsFemale ? textObject2 : textObject1);
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(YouthAge));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        if (base.SelectedTitleType < 1 || base.SelectedTitleType > 10)
        {
            base.SelectedTitleType = 1;
        }

        RefreshPlayerAppearance(characterCreation);
    }

    private void RefreshPlayerAppearance(CharacterCreation characterCreation)
    {
        var outfit = "player_char_creation_" + GetSelectedCulture().StringId + "_" + base.SelectedTitleType + (Hero.MainHero.IsFemale ? "_f" : "_m");
        ChangePlayerOutfit(characterCreation, outfit);
        ApplyEquipments(characterCreation);
    }

    private void AccomplishmentOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(AccomplishmentAge));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        RefreshPlayerAppearance(characterCreation);
    }

    private void StartingAgeOnInit(CharacterCreation characterCreation)
    {
        characterCreation.IsPlayerAlone = true;
        characterCreation.HasSecondaryCharacter = false;
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge((float)StartingAge));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_schooled" });
        RefreshPlayerAppearance(characterCreation);
    }

    private void StartingAgeYoungOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(20f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_focus" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.YoungAdult;
        SetHeroAge(20f);
    }

    private void StartingAgeAdultOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(30f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_ready" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.Adult;
        SetHeroAge(30f);
    }

    private void StartingAgeMiddleAgedOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(40f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_sharp" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.MiddleAged;
        SetHeroAge(40f);
    }

    private void StartingAgeElderlyOnConsequence(CharacterCreation characterCreation)
    {
        characterCreation.ClearFaceGenPrefab();
        characterCreation.ChangeFaceGenChars(ChangePlayerFaceWithAge(50f));
        characterCreation.ChangeCharsAnimation(new List<string> { "act_childhood_tough" });
        RefreshPlayerAppearance(characterCreation);
        StartingAge = SandboxAgeOptions.Elder;
        SetHeroAge(50f);
    }

    private void StartingAgeYoungOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.YoungAdult;
    }

    private void StartingAgeAdultOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.Adult;
    }

    private void StartingAgeMiddleAgedOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.MiddleAged;
    }

    private void StartingAgeElderlyOnApply(CharacterCreation characterCreation)
    {
        StartingAge = SandboxAgeOptions.Elder;
    }

    private void ApplyEquipments(CharacterCreation characterCreation)
    {
        ClearMountEntity(characterCreation);
        var instance = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_" + GetSelectedCulture().StringId + "_" + base.SelectedTitleType + (Hero.MainHero.IsFemale ? "_f" : "_m"));
        base.PlayerStartEquipment = instance?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
        base.PlayerCivilianEquipment = instance?.GetCivilianEquipments().FirstOrDefault() ?? MBEquipmentRoster.EmptyEquipment;
        if (base.PlayerStartEquipment != null && base.PlayerCivilianEquipment != null)
        {
            CharacterObject.PlayerCharacter.Equipment.FillFrom(base.PlayerStartEquipment);
            CharacterObject.PlayerCharacter.FirstCivilianEquipment.FillFrom(base.PlayerCivilianEquipment);
        }

        ChangePlayerMount(characterCreation, Hero.MainHero);
    }

    private void SetHeroAge(float age)
    {
        Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(0f - age));
    }
}
