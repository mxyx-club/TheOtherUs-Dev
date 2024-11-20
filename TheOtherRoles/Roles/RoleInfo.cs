using System.Collections.Generic;
using System.Linq;
using InnerNet;
using TheOtherRoles.Buttons;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Roles;

public class RoleInfo(string name, Color color, RoleId roleId, RoleType roleTeam, bool isGuessable = false)
{
    public string Name => getString(nameKey);
    public string IntroDescription => getString(nameKey + "IntroDesc");
    public string ShortDescription => getString(nameKey + "ShortDesc");
    public string FullDescription => getString(nameKey + "FullDesc");

    public Color color = color;
    public RoleId roleId = roleId;
    public RoleType roleTeam = roleTeam;
    public bool isGuessable = isGuessable;
    private readonly string nameKey = name;

    public static RoleInfo impostor = new("Impostor", Palette.ImpostorRed, RoleId.Impostor, RoleType.Impostor);
    public static RoleInfo morphling = new("Morphling", Morphling.color, RoleId.Morphling, RoleType.Impostor);
    public static RoleInfo bomber = new("Bomber", Bomber.color, RoleId.Bomber, RoleType.Impostor);
    public static RoleInfo poucher = new("Poucher", Poucher.color, RoleId.Poucher, RoleType.Impostor);
    public static RoleInfo butcher = new("Butcher", Eraser.color, RoleId.Butcher, RoleType.Impostor);
    public static RoleInfo mimic = new("Mimic", Mimic.color, RoleId.Mimic, RoleType.Impostor);
    public static RoleInfo camouflager = new("Camouflager", Camouflager.color, RoleId.Camouflager, RoleType.Impostor);
    public static RoleInfo miner = new("Miner", Miner.color, RoleId.Miner, RoleType.Impostor);
    public static RoleInfo eraser = new("Eraser", Eraser.color, RoleId.Eraser, RoleType.Impostor);
    public static RoleInfo vampire = new("Vampire", Vampire.color, RoleId.Vampire, RoleType.Impostor);
    public static RoleInfo cleaner = new("Cleaner", Cleaner.color, RoleId.Cleaner, RoleType.Impostor);
    public static RoleInfo undertaker = new("Undertaker", Undertaker.color, RoleId.Undertaker, RoleType.Impostor);
    public static RoleInfo escapist = new("Escapist", Escapist.color, RoleId.Escapist, RoleType.Impostor);
    public static RoleInfo warlock = new("Warlock", Warlock.color, RoleId.Warlock, RoleType.Impostor);
    public static RoleInfo trickster = new("Trickster", Trickster.color, RoleId.Trickster, RoleType.Impostor);
    public static RoleInfo bountyHunter = new("BountyHunter", BountyHunter.color, RoleId.BountyHunter, RoleType.Impostor);
    public static RoleInfo terrorist = new("Terrorist", Terrorist.color, RoleId.Terrorist, RoleType.Impostor);
    public static RoleInfo blackmailer = new("Blackmailer", Blackmailer.color, RoleId.Blackmailer, RoleType.Impostor);
    public static RoleInfo witch = new("Witch", Witch.color, RoleId.Witch, RoleType.Impostor);
    public static RoleInfo ninja = new("Ninja", Ninja.color, RoleId.Ninja, RoleType.Impostor);
    public static RoleInfo yoyo = new("Yoyo", Yoyo.color, RoleId.Yoyo, RoleType.Impostor);
    public static RoleInfo evilTrapper = new("EvilTrapper", EvilTrapper.color, RoleId.EvilTrapper, RoleType.Impostor);
    public static RoleInfo gambler = new("Gambler", Gambler.color, RoleId.Gambler, RoleType.Impostor);
    public static RoleInfo grenadier = new("Grenadier", Grenadier.color, RoleId.Grenadier, RoleType.Impostor);

    public static RoleInfo survivor = new("Survivor", Survivor.color, RoleId.Survivor, RoleType.Neutral);
    public static RoleInfo amnisiac = new("Amnisiac", Amnisiac.color, RoleId.Amnisiac, RoleType.Neutral);
    public static RoleInfo jester = new("Jester", Jester.color, RoleId.Jester, RoleType.Neutral);
    public static RoleInfo vulture = new("Vulture", Vulture.color, RoleId.Vulture, RoleType.Neutral);
    public static RoleInfo lawyer = new("Lawyer", Lawyer.color, RoleId.Lawyer, RoleType.Neutral);
    public static RoleInfo executioner = new("Executioner", Executioner.color, RoleId.Executioner, RoleType.Neutral);
    public static RoleInfo pursuer = new("Pursuer", Pursuer.color, RoleId.Pursuer, RoleType.Neutral);
    public static RoleInfo partTimer = new("PartTimer", PartTimer.color, RoleId.PartTimer, RoleType.Neutral);
    public static RoleInfo jackal = new("Jackal", Jackal.color, RoleId.Jackal, RoleType.Neutral);
    public static RoleInfo sidekick = new("Sidekick", Sidekick.color, RoleId.Sidekick, RoleType.Neutral);
    public static RoleInfo pavlovsowner = new("Pavlovsowner", Pavlovsdogs.color, RoleId.Pavlovsowner, RoleType.Neutral);
    public static RoleInfo pavlovsdogs = new("Pavlovsdogs", Pavlovsdogs.color, RoleId.Pavlovsdogs, RoleType.Neutral);
    public static RoleInfo swooper = new("Swooper", Swooper.color, RoleId.Swooper, RoleType.Neutral);
    public static RoleInfo arsonist = new("Arsonist", Arsonist.color, RoleId.Arsonist, RoleType.Neutral);
    public static RoleInfo werewolf = new("Werewolf", Werewolf.color, RoleId.Werewolf, RoleType.Neutral);
    public static RoleInfo thief = new("Thief", Thief.color, RoleId.Thief, RoleType.Neutral);
    public static RoleInfo juggernaut = new("Juggernaut", Juggernaut.color, RoleId.Juggernaut, RoleType.Neutral);
    public static RoleInfo doomsayer = new("Doomsayer", Doomsayer.color, RoleId.Doomsayer, RoleType.Neutral);
    public static RoleInfo akujo = new("Akujo", Akujo.color, RoleId.Akujo, RoleType.Neutral);

    public static RoleInfo crewmate = new("Crewmate", Color.white, RoleId.Crewmate, RoleType.Crewmate);
    public static RoleInfo vigilante = new("Vigilante", Vigilante.color, RoleId.Vigilante, RoleType.Crewmate);
    public static RoleInfo mayor = new("Mayor", Mayor.color, RoleId.Mayor, RoleType.Crewmate);
    public static RoleInfo prosecutor = new("Prosecutor", Prosecutor.color, RoleId.Prosecutor, RoleType.Crewmate);
    public static RoleInfo portalmaker = new("Portalmaker", Portalmaker.color, RoleId.Portalmaker, RoleType.Crewmate);
    public static RoleInfo engineer = new("Engineer", Engineer.color, RoleId.Engineer, RoleType.Crewmate);
    public static RoleInfo sheriff = new("Sheriff", Sheriff.color, RoleId.Sheriff, RoleType.Crewmate);
    public static RoleInfo deputy = new("Deputy", Deputy.color, RoleId.Deputy, RoleType.Crewmate);
    public static RoleInfo bodyguard = new("BodyGuard", BodyGuard.color, RoleId.BodyGuard, RoleType.Crewmate);
    public static RoleInfo jumper = new("Jumper", Jumper.color, RoleId.Jumper, RoleType.Crewmate);
    public static RoleInfo detective = new("Detective", Detective.color, RoleId.Detective, RoleType.Crewmate);
    public static RoleInfo timeMaster = new("TimeMaster", TimeMaster.color, RoleId.TimeMaster, RoleType.Crewmate);
    public static RoleInfo veteran = new("Veteran", Veteran.color, RoleId.Veteran, RoleType.Crewmate);
    public static RoleInfo medic = new("Medic", Medic.color, RoleId.Medic, RoleType.Crewmate);
    public static RoleInfo swapper = new("Swapper", Swapper.color, RoleId.Swapper, RoleType.Crewmate);
    public static RoleInfo seer = new("Seer", Seer.color, RoleId.Seer, RoleType.Crewmate);
    public static RoleInfo hacker = new("Hacker", Hacker.color, RoleId.Hacker, RoleType.Crewmate);
    public static RoleInfo tracker = new("Tracker", Tracker.color, RoleId.Tracker, RoleType.Crewmate);
    public static RoleInfo snitch = new("Snitch", Snitch.color, RoleId.Snitch, RoleType.Crewmate);
    public static RoleInfo prophet = new("Prophet", Prophet.color, RoleId.Prophet, RoleType.Crewmate);
    public static RoleInfo infoSleuth = new("InfoSleuth", InfoSleuth.color, RoleId.InfoSleuth, RoleType.Crewmate);
    public static RoleInfo spy = new("Spy", Spy.color, RoleId.Spy, RoleType.Crewmate);
    public static RoleInfo securityGuard = new("SecurityGuard", SecurityGuard.color, RoleId.SecurityGuard, RoleType.Crewmate);
    public static RoleInfo medium = new("Medium", Medium.color, RoleId.Medium, RoleType.Crewmate);
    public static RoleInfo trapper = new("Trapper", Trapper.color, RoleId.Trapper, RoleType.Crewmate);
    public static RoleInfo balancer = new("Balancer", Balancer.color, RoleId.Balancer, RoleType.Crewmate);

    // Modifier
    public static RoleInfo assassin = new("Assassin", Assassin.color, RoleId.Assassin, RoleType.Modifier);
    public static RoleInfo lover = new("Lover", Lovers.color, RoleId.Lover, RoleType.Modifier, true);
    public static RoleInfo disperser = new("Disperser", Disperser.color, RoleId.Disperser, RoleType.Modifier, true);
    public static RoleInfo specoality = new("Specoality", Specoality.color, RoleId.Specoality, RoleType.Modifier);
    public static RoleInfo poucherModifier = new("Poucher", Poucher.color, RoleId.PoucherModifier, RoleType.Modifier);
    public static RoleInfo lastImpostor = new("LastImpostor", LastImpostor.color, RoleId.LastImpostor, RoleType.Modifier);
    public static RoleInfo bloody = new("Bloody", Color.yellow, RoleId.Bloody, RoleType.Modifier, true);
    public static RoleInfo antiTeleport = new("AntiTeleport", Color.yellow, RoleId.AntiTeleport, RoleType.Modifier);
    public static RoleInfo tiebreaker = new("TieBreaker", Color.yellow, RoleId.Tiebreaker, RoleType.Modifier, true);
    public static RoleInfo aftermath = new("Aftermath", Color.yellow, RoleId.Aftermath, RoleType.Modifier, true);
    public static RoleInfo bait = new("Bait", Color.yellow, RoleId.Bait, RoleType.Modifier, true);
    public static RoleInfo sunglasses = new("Sunglasses", Color.yellow, RoleId.Sunglasses, RoleType.Modifier);
    public static RoleInfo torch = new("Torch", Color.yellow, RoleId.Torch, RoleType.Modifier, true);
    public static RoleInfo flash = new("Flash", Color.yellow, RoleId.Flash, RoleType.Modifier);
    public static RoleInfo multitasker = new("Multitasker", Color.yellow, RoleId.Multitasker, RoleType.Modifier, true);
    public static RoleInfo giant = new("Giant", Color.yellow, RoleId.Giant, RoleType.Modifier);
    public static RoleInfo mini = new("Mini", Color.yellow, RoleId.Mini, RoleType.Modifier);
    public static RoleInfo vip = new("Vip", Color.yellow, RoleId.Vip, RoleType.Modifier, true);
    public static RoleInfo indomitable = new("Indomitable", Color.yellow, RoleId.Indomitable, RoleType.Modifier);
    public static RoleInfo slueth = new("Slueth", Color.yellow, RoleId.Slueth, RoleType.Modifier, true);
    public static RoleInfo cursed = new("Cursed", Color.yellow, RoleId.Cursed, RoleType.Modifier, true);
    public static RoleInfo invert = new("Invert", Color.yellow, RoleId.Invert, RoleType.Modifier);
    public static RoleInfo blind = new("Blind", Color.yellow, RoleId.Blind, RoleType.Modifier);
    public static RoleInfo watcher = new("Watcher", Color.yellow, RoleId.Watcher, RoleType.Modifier, true);
    public static RoleInfo radar = new("Radar", Color.yellow, RoleId.Radar, RoleType.Modifier, true);
    public static RoleInfo tunneler = new("Tunneler", Color.yellow, RoleId.Tunneler, RoleType.Modifier, true);
    public static RoleInfo buttonBarry = new("ButtonBarry", Color.yellow, RoleId.ButtonBarry, RoleType.Modifier);
    public static RoleInfo chameleon = new("Chameleon", Color.yellow, RoleId.Chameleon, RoleType.Modifier);
    public static RoleInfo shifter = new("Shifter", Color.yellow, RoleId.Shifter, RoleType.Modifier);

    public static List<RoleInfo> allRoleInfos =
    [
        impostor,
        morphling,
        bomber,
        poucher,
        butcher,
        mimic,
        camouflager,
        miner,
        eraser,
        vampire,
        undertaker,
        escapist,
        warlock,
        trickster,
        bountyHunter,
        cleaner,
        terrorist,
        blackmailer,
        witch,
        ninja,
        yoyo,
        evilTrapper,
        gambler,
        grenadier,

        survivor,
        amnisiac,
        jester,
        vulture,
        lawyer,
        executioner,
        pursuer,
        partTimer,
        doomsayer,
        arsonist,
        jackal,
        sidekick,
        pavlovsowner,
        pavlovsdogs,
        werewolf,
        swooper,
        juggernaut,
        akujo,
        thief,

        crewmate,
        vigilante,
        mayor,
        prosecutor,
        portalmaker,
        engineer,
        sheriff,
        deputy,
        bodyguard,
        jumper,
        detective,
        medic,
        timeMaster,
        veteran,
        swapper,
        seer,
        hacker,
        tracker,
        snitch,
        prophet,
        infoSleuth,
        spy,
        securityGuard,
        medium,
        trapper,
        balancer,

        lover,
        assassin,
        poucherModifier,
        disperser,
        specoality,
        lastImpostor,
        bloody,
        antiTeleport,
        tiebreaker,
        aftermath,
        bait,
        flash,
        torch,
        sunglasses,
        multitasker,
        mini,
        giant,
        vip,
        indomitable,
        slueth,
        cursed,
        invert,
        blind,
        watcher,
        radar,
        tunneler,
        buttonBarry,
        chameleon,
        shifter,
    ];

    public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, bool showModifier = true)
    {
        var infos = new List<RoleInfo>();
        if (p == null) return infos;

        // Modifier
        if (showModifier)
        {
            // after dead modifier
            if (!CustomOptionHolder.modifiersAreHidden.getBool() || CachedPlayer.LocalPlayer.IsDead ||
                AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended)
            {
                if (Bait.bait.Any(x => x.PlayerId == p.PlayerId)) infos.Add(bait);
                if (Bloody.bloody.Any(x => x.PlayerId == p.PlayerId)) infos.Add(bloody);
                if (Vip.vip.Any(x => x.PlayerId == p.PlayerId)) infos.Add(vip);
                if (p == Tiebreaker.tiebreaker) infos.Add(tiebreaker);
                if (p == Indomitable.indomitable) infos.Add(indomitable);
                if (p == Aftermath.aftermath) infos.Add(aftermath);
                if (p == Cursed.cursed && !Cursed.hideModifier) infos.Add(cursed);
            }
            if (p == Lovers.lover1 || p == Lovers.lover2) infos.Add(lover);
            if (Assassin.assassin.Any(x => x.PlayerId == p.PlayerId) && p != Specoality.specoality) infos.Add(assassin);
            if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == p.PlayerId)) infos.Add(antiTeleport);
            if (Sunglasses.sunglasses.Any(x => x.PlayerId == p.PlayerId)) infos.Add(sunglasses);
            if (Torch.torch.Any(x => x.PlayerId == p.PlayerId)) infos.Add(torch);
            if (Flash.flash.Any(x => x.PlayerId == p.PlayerId)) infos.Add(flash);
            if (Multitasker.multitasker.Any(x => x.PlayerId == p.PlayerId)) infos.Add(multitasker);
            if (p == Mini.mini) infos.Add(mini);
            if (p == Blind.blind) infos.Add(blind);
            if (p == Watcher.watcher) infos.Add(watcher);
            if (p == Radar.radar) infos.Add(radar);
            if (p == Tunneler.tunneler) infos.Add(tunneler);
            if (p == ButtonBarry.buttonBarry) infos.Add(buttonBarry);
            if (p == Slueth.slueth) infos.Add(slueth);
            if (p == Disperser.disperser) infos.Add(disperser);
            if (p == Specoality.specoality) infos.Add(specoality);
            if (p == Poucher.poucher && Poucher.spawnModifier) infos.Add(poucherModifier);
            if (p == Giant.giant) infos.Add(giant);
            if (Invert.invert.Any(x => x.PlayerId == p.PlayerId)) infos.Add(invert);
            if (Chameleon.chameleon.Any(x => x.PlayerId == p.PlayerId)) infos.Add(chameleon);
            if (p == Shifter.shifter) infos.Add(shifter);
            if (p == LastImpostor.lastImpostor) infos.Add(lastImpostor);
        }

        var count = infos.Count; // Save count after modifiers are added so that the role count can be checked

        // Special roles
        if (p == Mimic.mimic) infos.Add(mimic);
        if (p == Jester.jester) infos.Add(jester);
        if (p == Swooper.swooper) infos.Add(swooper);
        if (p == Werewolf.werewolf) infos.Add(werewolf);
        if (p == Miner.miner) infos.Add(miner);
        if (p == Poucher.poucher && !Poucher.spawnModifier) infos.Add(poucher);
        if (p == Butcher.butcher) infos.Add(butcher);
        if (p == Morphling.morphling) infos.Add(morphling);
        if (p == Bomber.bomber) infos.Add(bomber);
        if (p == Camouflager.camouflager) infos.Add(camouflager);
        if (p == Vampire.vampire) infos.Add(vampire);
        if (p == Eraser.eraser) infos.Add(eraser);
        if (p == Trickster.trickster) infos.Add(trickster);
        if (p == Cleaner.cleaner) infos.Add(cleaner);
        if (p == Undertaker.undertaker) infos.Add(undertaker);
        if (p == Warlock.warlock) infos.Add(warlock);
        if (p == Witch.witch) infos.Add(witch);
        if (p == Escapist.escapist) infos.Add(escapist);
        if (p == Gambler.gambler) infos.Add(gambler);
        if (p == Ninja.ninja) infos.Add(ninja);
        if (p == Yoyo.yoyo) infos.Add(yoyo);
        if (p == EvilTrapper.evilTrapper) infos.Add(evilTrapper);
        if (p == Blackmailer.blackmailer) infos.Add(blackmailer);
        if (p == Terrorist.terrorist) infos.Add(terrorist);
        if (p == Detective.detective) infos.Add(detective);
        if (p == TimeMaster.timeMaster) infos.Add(timeMaster);
        if (p == Amnisiac.amnisiac) infos.Add(amnisiac);
        if (p == Veteran.veteran) infos.Add(veteran);
        if (p == Grenadier.grenadier) infos.Add(grenadier);
        if (p == Medic.medic) infos.Add(medic);
        if (p == Swapper.swapper) infos.Add(swapper);
        if (p == BodyGuard.bodyguard) infos.Add(bodyguard);
        if (p == Seer.seer) infos.Add(seer);
        if (p == Hacker.hacker) infos.Add(hacker);
        if (p == Tracker.tracker) infos.Add(tracker);
        if (p == Snitch.snitch) infos.Add(snitch);
        if (p == Jackal.jackal || (Jackal.formerJackals != null && Jackal.formerJackals.Any(x => x.PlayerId == p.PlayerId))) infos.Add(jackal);
        if (p == Sidekick.sidekick) infos.Add(sidekick);
        if (p == Spy.spy) infos.Add(spy);
        if (p == SecurityGuard.securityGuard) infos.Add(securityGuard);
        if (p == Arsonist.arsonist) infos.Add(arsonist);
        if (p == Vigilante.vigilante) infos.Add(vigilante);
        if (p == Mayor.mayor) infos.Add(mayor);
        if (p == Portalmaker.portalmaker) infos.Add(portalmaker);
        if (p == Engineer.engineer) infos.Add(engineer);
        if (p == Sheriff.sheriff || p == Sheriff.formerSheriff) infos.Add(sheriff);
        if (p == Deputy.deputy) infos.Add(deputy);
        if (p == BountyHunter.bountyHunter) infos.Add(bountyHunter);
        if (p == Vulture.vulture) infos.Add(vulture);
        if (p == Medium.medium) infos.Add(medium);
        if (p == Lawyer.lawyer) infos.Add(lawyer);
        if (p == PartTimer.partTimer) infos.Add(partTimer);
        if (p == Prosecutor.prosecutor) infos.Add(prosecutor);
        if (p == Balancer.balancer) infos.Add(balancer);
        if (p == Executioner.executioner) infos.Add(executioner);
        if (p == Trapper.trapper) infos.Add(trapper);
        if (p == Prophet.prophet) infos.Add(prophet);
        if (p == InfoSleuth.infoSleuth) infos.Add(infoSleuth);
        if (p == Jumper.jumper) infos.Add(jumper);
        if (p == Thief.thief) infos.Add(thief);
        if (p == Juggernaut.juggernaut) infos.Add(juggernaut);
        if (p == Doomsayer.doomsayer) infos.Add(doomsayer);
        if (p == Akujo.akujo) infos.Add(akujo);
        if (p == Pavlovsdogs.pavlovsowner) infos.Add(pavlovsowner);
        if (p == Pavlovsdogs.pavlovsdogs.Any(x => x.PlayerId == p.PlayerId)) infos.Add(pavlovsdogs);
        if (Pursuer.pursuer.Any(x => x.PlayerId == p.PlayerId)) infos.Add(pursuer);
        if (Survivor.survivor.Any(x => x.PlayerId == p.PlayerId)) infos.Add(survivor);

        // Default roles (just impostor, just crewmate, or hunter / hunted for hide n seek, prop hunt prop ...
        return infos;
    }

    public static string GetRolesString(PlayerControl p, bool useColors, bool showModifier = true, bool suppressGhostInfo = false)
    {
        string roleName;
        roleName = string.Join(" ", getRoleInfoForPlayer(p, showModifier).Select(x => useColors ? cs(x.color, x.Name) : x.Name).ToArray());
        if (Lawyer.target != null && p.PlayerId == Lawyer.target.PlayerId &&
            CachedPlayer.LocalPlayer.PlayerControl != Lawyer.target) roleName += useColors ? cs(Lawyer.color, " §") : " §";

        if (Executioner.target != null && p.PlayerId == Executioner.target.PlayerId &&
            CachedPlayer.LocalPlayer.PlayerControl != Executioner.target) roleName += useColors ? cs(Executioner.color, " §") : " §";

        if (p == Jackal.jackal && Jackal.canSwoop) roleName += "JackalIsSwooperInfo".Translate();

        if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(p.PlayerId) && p != Specoality.specoality && p != Doomsayer.doomsayer) roleName += "GuessserGMInfo".Translate();

        if (!suppressGhostInfo && p != null)
        {
            if (p == Shifter.shifter &&
                (CachedPlayer.LocalPlayer.PlayerControl == Shifter.shifter || shouldShowGhostInfo()) &&
                Shifter.futureShift != null)
                roleName += cs(Color.yellow, " ← " + Shifter.futureShift.Data.PlayerName);
            if (p == Vulture.vulture && (CachedPlayer.LocalPlayer.PlayerControl == Vulture.vulture || shouldShowGhostInfo()))
                roleName += cs(Vulture.color, string.Format("roleInfoRemaining".Translate(), Vulture.vultureNumberToWin - Vulture.eatenBodies));
            if (shouldShowGhostInfo())
            {
                if (Eraser.futureErased.Contains(p))
                    roleName = cs(Color.gray, "(被抹除) ") + roleName;
                if (Vampire.vampire != null && !Vampire.vampire.Data.IsDead && Vampire.bitten == p && !p.Data.IsDead)
                    roleName = cs(Vampire.color,
                        $"(被吸血 {(int)HudManagerStartPatch.vampireKillButton.Timer + 1}) ") + roleName;
                if (Deputy.handcuffedPlayers.Contains(p.PlayerId))
                    roleName = cs(Color.gray, "(被上拷) ") + roleName;
                if (Deputy.handcuffedKnows.ContainsKey(p.PlayerId)) // Active cuff
                    roleName = cs(Deputy.color, "(被上拷) ") + roleName;
                if (p == Warlock.curseVictim)
                    roleName = cs(Warlock.color, "(被下咒) ") + roleName;
                if (p == Ninja.ninjaMarked)
                    roleName = cs(Ninja.color, "(被标记) ") + roleName;
                if (Pursuer.blankedList.Contains(p) && !p.Data.IsDead)
                    roleName = cs(Pursuer.color, "(被塞空包弹) ") + roleName;
                if (Witch.futureSpelled.Contains(p) && !MeetingHud.Instance) // This is already displayed in meetings!
                    roleName = cs(Witch.color, "☆ ") + roleName;
                if (BountyHunter.bounty == p)
                    roleName = cs(BountyHunter.color, "(被悬赏) ") + roleName;
                //if (Arsonist.dousedPlayers.Contains(p))
                //    roleName = cs(Arsonist.color, "♨ ") + roleName;
                if (p == Arsonist.arsonist)
                    roleName += cs(Arsonist.color,
                        $" (剩余 {CachedPlayer.AllPlayers.Count(x => { return x.PlayerControl != Arsonist.arsonist && !x.Data.IsDead && !x.Data.Disconnected && !Arsonist.dousedPlayers.Any(y => y.PlayerId == x.PlayerId); })} )");
                if (Akujo.keeps.Contains(p))
                    roleName = cs(Color.gray, "(备胎) ") + roleName;
                if (p == Akujo.honmei)
                    roleName = cs(Akujo.color, "(真爱) ") + roleName;

                // Death Reason on Ghosts
                if (p.Data.IsDead)
                {
                    var deathReasonString = "";
                    GameHistory.AllDeadPlayers.TryGetValue(p.PlayerId, out var deadPlayer);

                    Color killerColor = new();
                    if (deadPlayer != null && deadPlayer.KillerIfExisting != null)
                        killerColor = getRoleInfoForPlayer(deadPlayer.KillerIfExisting, false).FirstOrDefault().color;

                    if (deadPlayer != null)
                    {
                        switch (deadPlayer.DeathReason)
                        {
                            case CustomDeathReason.Disconnect:
                                deathReasonString = " - 断开连接";
                                break;
                            case CustomDeathReason.HostCmdKill:
                                deathReasonString = $" - 被 {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)} 制裁";
                                break;
                            case CustomDeathReason.SheriffKill:
                                deathReasonString = $" - 出警 {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                            case CustomDeathReason.SheriffMisfire:
                                deathReasonString = " - 走火";
                                break;
                            case CustomDeathReason.SheriffMisadventure:
                                deathReasonString = $" - 被误杀于 {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                            case CustomDeathReason.Suicide:
                                deathReasonString = " - 自杀";
                                break;
                            case CustomDeathReason.BombVictim:
                                deathReasonString = " - 恐袭";
                                break;
                            case CustomDeathReason.Exile:
                                deathReasonString = " - 被驱逐";
                                break;
                            case CustomDeathReason.Kill:
                                deathReasonString =
                                    $" - 被击杀于 {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                            case CustomDeathReason.Guess:
                                if (deadPlayer.KillerIfExisting.Data.PlayerName == p.Data.PlayerName)
                                    deathReasonString = " - 猜测错误";
                                else
                                    deathReasonString =
                                        $" - 被赌杀于 {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                            case CustomDeathReason.Shift:
                                deathReasonString =
                                    $" - {cs(Color.yellow, "交换")} {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)} 失败";
                                break;
                            case CustomDeathReason.WitchExile:
                                deathReasonString =
                                    $" - {cs(Witch.color, "被咒杀于")} {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                            case CustomDeathReason.LoverSuicide:
                                deathReasonString = $" - {cs(Lovers.color, "殉情")}";
                                break;
                            case CustomDeathReason.LawyerSuicide:
                                deathReasonString = $" - {cs(Lawyer.color, "辩护失败")}";
                                break;
                            case CustomDeathReason.Bomb:
                                deathReasonString =
                                    $" - 被恐袭于 {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                            case CustomDeathReason.Arson:
                                deathReasonString =
                                    $" - 被烧死于 {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                            case CustomDeathReason.LoveStolen:
                                deathReasonString = $" - {cs(Lovers.color, "爱人被夺")}";
                                break;
                            case CustomDeathReason.Loneliness:
                                deathReasonString = $" - {cs(Akujo.color, "精力衰竭")}";
                                break;
                            case CustomDeathReason.FakeSK:
                                deathReasonString = $" - {cs(Jackal.color, "招募失败")} {cs(killerColor, deadPlayer.KillerIfExisting.Data.PlayerName)}";
                                break;
                        }
                        roleName += deathReasonString;
                    }
                }
            }
        }

        return roleName;
    }

    public static string getRoleDescription(string name)
    {
        foreach (var roleInfo in allRoleInfos)
        {
            if (roleInfo.Name == name) return $"{name}: \n{$"{roleInfo.nameKey}FullDesc".Translate()}";
        }
        return null;
    }
}
