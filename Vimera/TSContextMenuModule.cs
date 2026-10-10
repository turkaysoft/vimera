using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Windows.Forms;
// TS Modules
using static Vimera.TSModules;

namespace Vimera{
    internal static class TSContextMenuModule{
        // PIPE / MUTEX NAMES (classic right-click integration, HKCU only)
        // ======================================================================================================
        public const string PipeName = "VimeraHashPipe";
        public const string MutexName = @"Local\VimeraSingleInstance";
        public const string CtxRootName = "Vimera";
        // HASH DEFINITIONS (FileHash tab order: 0=CRC32 1=CRC64 2=MD5 3=SHA1 4=SHA256 5=SHA384 6=SHA512)
        // ======================================================================================================
        public struct CtxHashDef{
            public string Id;
            public string CliKey;
            public string Display;
            public int Mode;
        }
        public static readonly CtxHashDef[] HashDefs = new CtxHashDef[]{
            new CtxHashDef{ Id = "Vimera.CRC32", CliKey = "CRC32", Display = "CRC-32", Mode = 0 },
            new CtxHashDef{ Id = "Vimera.CRC64", CliKey = "CRC64", Display = "CRC-64", Mode = 1 },
            new CtxHashDef{ Id = "Vimera.MD5", CliKey = "MD5", Display = "MD5", Mode = 2 },
            new CtxHashDef{ Id = "Vimera.SHA1", CliKey = "SHA1", Display = "SHA-1", Mode = 3 },
            new CtxHashDef{ Id = "Vimera.SHA256", CliKey = "SHA256", Display = "SHA-256", Mode = 4 },
            new CtxHashDef{ Id = "Vimera.SHA384", CliKey = "SHA384", Display = "SHA-384", Mode = 5 },
            new CtxHashDef{ Id = "Vimera.SHA512", CliKey = "SHA512", Display = "SHA-512", Mode = 6 },
        };
        public static int HashKeyToMode(string key){
            try{
                if (string.IsNullOrWhiteSpace(key))
                    return -1;
                string n = key.Trim().ToUpperInvariant().Replace("-", "").Replace("_", "").Replace(" ", "");
                if (n == "CRC32") return 0;
                if (n == "CRC64") return 1;
                if (n == "MD5") return 2;
                if (n == "SHA1") return 3;
                if (n == "SHA256") return 4;
                if (n == "SHA384") return 5;
                if (n == "SHA512") return 6;
                return -1;
            }catch{ return -1; }
        }
        // CONTEXT MENU LABELS ([ContextMenu] section of the active language file)
        // ======================================================================================================
        public class CtxLabels{
            public string Root = string.Empty;
            public Dictionary<int, string> Subs = new Dictionary<int, string>();
        }
        private static string SafeFormatLabel(string template, string arg, string fallback){
            try{
                if (string.IsNullOrWhiteSpace(template) || template == "N/A Langs")
                    return fallback;
                if (template.IndexOf("{0}") < 0)
                    return template.Trim();
                return string.Format(template, arg).Trim();
            }catch{ return fallback; }
        }
        public static CtxLabels DefaultLabels(string appName){
            CtxLabels labels = new CtxLabels();
            try{
                string app = string.IsNullOrWhiteSpace(appName) ? CtxRootName : appName.Trim();
                labels.Root = app;
                for (int i = 0; i < HashDefs.Length; i++)
                    labels.Subs[HashDefs[i].Mode] = HashDefs[i].Display;
            }catch{ }
            return labels;
        }
        public static CtxLabels BuildLabels(TSGetLangs langs, string appName){
            CtxLabels labels = DefaultLabels(appName);
            try{
                if (langs == null)
                    return labels;
                string app = string.IsNullOrWhiteSpace(appName) ? CtxRootName : appName.Trim();
                labels.Root = SafeFormatLabel(langs.TSReadLangs("ContextMenu", "ctx_open_with"), app, app);
                string hashTpl = langs.TSReadLangs("ContextMenu", "ctx_hash_action");
                for (int i = 0; i < HashDefs.Length; i++)
                    labels.Subs[HashDefs[i].Mode] = SafeFormatLabel(hashTpl, HashDefs[i].Display, HashDefs[i].Display);
            }catch{ }
            return labels;
        }
        private static string LabelFor(CtxLabels labels, int mode, string fallback){
            try{
                if (labels != null && labels.Subs != null && labels.Subs.ContainsKey(mode)){
                    string label = labels.Subs[mode];
                    if (!string.IsNullOrWhiteSpace(label))
                        return label;
                }
            }catch{ }
            return fallback;
        }
        // CLI PARSE RESULT
        // ======================================================================================================
        public class CtxLaunchArgs{
            public int HashMode = -1;
            public List<string> Files = new List<string>();
        }
        public static CtxLaunchArgs ParseArgs(string[] args){
            CtxLaunchArgs result = new CtxLaunchArgs();
            try{
                if (args == null || args.Length == 0)
                    return result;
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < args.Length; i++){
                    string raw = args[i];
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    string t = raw.Trim().Trim('"');
                    if (t.Length == 0)
                        continue;
                    // --hash=VALUE / -hash=VALUE / /hash:VALUE / --hash VALUE
                    string low = t.TrimStart('-', '/');
                    bool isHashFlag = low.Equals("hash", StringComparison.OrdinalIgnoreCase);
                    bool isHashValue = low.StartsWith("hash=", StringComparison.OrdinalIgnoreCase) || low.StartsWith("hash:", StringComparison.OrdinalIgnoreCase);
                    if (isHashValue){
                        int eq = low.IndexOfAny(new char[]{ '=', ':' });
                        if (eq >= 0 && eq + 1 < low.Length){
                            int m = HashKeyToMode(low.Substring(eq + 1));
                            if (m >= 0)
                                result.HashMode = m;
                        }
                        continue;
                    }
                    if (isHashFlag){
                        if (i + 1 < args.Length){
                            int m = HashKeyToMode(args[i + 1].Trim().Trim('"'));
                            if (m >= 0){
                                result.HashMode = m;
                                i++;
                                continue;
                            }
                        }
                        continue;
                    }
                    // file / directory candidate
                    string candidate = t;
                    try{
                        if (File.Exists(candidate) || Directory.Exists(candidate)){
                            if (!seen.Contains(candidate)){
                                seen.Add(candidate);
                                result.Files.Add(candidate);
                            }
                            continue;
                        }
                        // absolute path look-alike (deleted between click and launch) -> keep, UI filters later
                        bool looksLikePath = candidate.IndexOf('\\') >= 0 || candidate.IndexOf('/') >= 0 || (candidate.Length > 2 && candidate[1] == ':');
                        if (looksLikePath){
                            if (!seen.Contains(candidate)){
                                seen.Add(candidate);
                                result.Files.Add(candidate);
                            }
                        }
                    }catch{ }
                }
            }catch{ }
            return result;
        }
        // REGISTRY (HKCU only, no admin required)
        // Per-user cascade recipe (MSDN: CommandStore method requires HKLM, HKCU CommandStore is ignored):
        //   AllFilesystemObjects\shell\Vimera (single key covers files AND folders, incl. mixed selection)
        //   <root>                   : MUIVerb, Icon, SubCommands="" (empty!), MultiSelectModel=Player, no (Default), no command
        //   <root>\shell\<Id>        : @=Display, MUIVerb=Display, Icon, MultiSelectModel=Player
        //   <root>\shell\<Id>\command: @="exe" --hash=KEY "%1"
        // ======================================================================================================
        private static string GetExePath(){
            try{ return Application.ExecutablePath; }
            catch{ return string.Empty; }
        }
        private static string[] CtxRoots(){
            return new string[]{
                @"Software\Classes\AllFilesystemObjects\shell\" + CtxRootName
            };
        }
        // previous two-key layout (* + Directory), removed on migrate/unregister
        private static string[] LegacyCtxRoots(){
            return new string[]{
                @"Software\Classes\*\shell\" + CtxRootName,
                @"Software\Classes\Directory\shell\" + CtxRootName
            };
        }
        private static void DeleteLegacyCtxRoots(){
            try{
                string[] roots = LegacyCtxRoots();
                for (int i = 0; i < roots.Length; i++){
                    try{ Registry.CurrentUser.DeleteSubKeyTree(roots[i], false); }catch{ }
                }
            }catch{ }
        }
        private static void DeleteLegacyCommandStore(){
            try{
                for (int i = 0; i < HashDefs.Length; i++){
                    try{ Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\" + HashDefs[i].Id, false); }catch{ }
                }
            }catch{ }
        }
        public static bool IsRegistered(CtxLabels labels){
            try{
                string exe = GetExePath();
                if (string.IsNullOrWhiteSpace(exe))
                    return false;
                string expectedRoot = (labels != null && !string.IsNullOrWhiteSpace(labels.Root)) ? labels.Root : CtxRootName;
                string[] roots = CtxRoots();
                for (int r = 0; r < roots.Length; r++){
                    using (RegistryKey parent = Registry.CurrentUser.OpenSubKey(roots[r], false)){
                        if (parent == null)
                            return false;
                        // cascade marker must exist and be EMPTY (ID list or missing = old/broken -> repair)
                        if (!(parent.GetValue("SubCommands") is string sub) || sub.Length != 0)
                            return false;
                        // parent label must match the active UI language (language switch -> repair)
                        string rootVerb = Convert.ToString(parent.GetValue("MUIVerb"));
                        if (!string.Equals(rootVerb, expectedRoot, StringComparison.Ordinal))
                            return false;
                        // parent must not be a runnable verb
                        object def = parent.GetValue(null);
                        if (def != null && Convert.ToString(def).Length != 0)
                            return false;
                        string icon = Convert.ToString(parent.GetValue("Icon"));
                        if (string.IsNullOrWhiteSpace(icon) || icon.IndexOf(exe, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                        // single invocation for multi-select + visible beyond 15 selected files
                        string msm = Convert.ToString(parent.GetValue("MultiSelectModel"));
                        if (!string.Equals(msm, "Player", StringComparison.OrdinalIgnoreCase))
                            return false;
                        // no forced position: Explorer sorts automatically (stale Position -> repair once)
                        try{
                            if (parent.GetValue("Position") != null)
                                return false;
                        }catch{ }
                        try{
                            using (RegistryKey legacyCmd = parent.OpenSubKey("command")){
                                if (legacyCmd != null)
                                    return false;
                            }
                        }catch{ }
                    }
                    for (int i = 0; i < HashDefs.Length; i++){
                        string subPath = roots[r] + @"\shell\" + HashDefs[i].Id;
                        string expectedSub = LabelFor(labels, HashDefs[i].Mode, HashDefs[i].Display);
                        using (RegistryKey sub = Registry.CurrentUser.OpenSubKey(subPath, false)){
                            if (sub == null)
                                return false;
                            string verb = Convert.ToString(sub.GetValue(null));
                            if (!string.Equals(verb, expectedSub, StringComparison.Ordinal))
                                verb = Convert.ToString(sub.GetValue("MUIVerb"));
                            if (!string.Equals(verb, expectedSub, StringComparison.Ordinal))
                                return false;
                            string subMsm = Convert.ToString(sub.GetValue("MultiSelectModel"));
                            if (!string.Equals(subMsm, "Player", StringComparison.OrdinalIgnoreCase))
                                return false;
                        }
                        using (RegistryKey cmd = Registry.CurrentUser.OpenSubKey(subPath + @"\command", false)){
                            if (cmd == null)
                                return false;
                            string cmdLine = Convert.ToString(cmd.GetValue(null));
                            if (string.IsNullOrWhiteSpace(cmdLine) || cmdLine.IndexOf(exe, StringComparison.OrdinalIgnoreCase) < 0)
                                return false;
                            if (cmdLine.IndexOf(HashDefs[i].CliKey, StringComparison.OrdinalIgnoreCase) < 0)
                                return false;
                        }
                    }
                }
                return true;
            }catch{ return false; }
        }
        public static bool Register(CtxLabels labels){
            try{
                string exe = GetExePath();
                if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe))
                    return false;
                string quotedExe = "\"" + exe + "\"";
                string icon = quotedExe + ",0";
                string rootVerb = (labels != null && !string.IsNullOrWhiteSpace(labels.Root)) ? labels.Root : CtxRootName;
                // clean broken first-generation leftovers (CommandStore + SubCommands ID list)
                // and the previous two-key layout (* + Directory)
                DeleteLegacyCommandStore();
                DeleteLegacyCtxRoots();
                string[] roots = CtxRoots();
                for (int r = 0; r < roots.Length; r++){
                    try{
                        try{ Registry.CurrentUser.DeleteSubKeyTree(roots[r], false); }catch{ }
                        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(roots[r])){
                            if (key == null)
                                return false;
                            key.SetValue("MUIVerb", rootVerb, RegistryValueKind.String);
                            key.SetValue("Icon", icon, RegistryValueKind.String);
                            key.SetValue("SubCommands", "", RegistryValueKind.String);
                            key.SetValue("MultiSelectModel", "Player", RegistryValueKind.String);
                        }
                        for (int i = 0; i < HashDefs.Length; i++){
                            string subPath = roots[r] + @"\shell\" + HashDefs[i].Id;
                            string subVerb = LabelFor(labels, HashDefs[i].Mode, HashDefs[i].Display);
                            using (RegistryKey sub = Registry.CurrentUser.CreateSubKey(subPath)){
                                if (sub == null)
                                    return false;
                                sub.SetValue(null, subVerb, RegistryValueKind.String);
                                sub.SetValue("MUIVerb", subVerb, RegistryValueKind.String);
                                sub.SetValue("Icon", icon, RegistryValueKind.String);
                                sub.SetValue("MultiSelectModel", "Player", RegistryValueKind.String);
                            }
                            using (RegistryKey cmd = Registry.CurrentUser.CreateSubKey(subPath + @"\command")){
                                if (cmd == null)
                                    return false;
                                cmd.SetValue(null, quotedExe + " --hash=" + HashDefs[i].CliKey + " \"%1\"", RegistryValueKind.String);
                            }
                        }
                    }catch{ return false; }
                }
                return true;
            }catch{ return false; }
        }
        public static bool Unregister(){
            try{
                string[] roots = CtxRoots();
                for (int r = 0; r < roots.Length; r++){
                    try{ Registry.CurrentUser.DeleteSubKeyTree(roots[r], false); }catch{ }
                }
                DeleteLegacyCtxRoots();
                DeleteLegacyCommandStore();
                return true;
            }catch{ return false; }
        }
        public static void RepairIfNeeded(bool shouldBeOn, CtxLabels labels){
            try{
                if (shouldBeOn){
                    if (!IsRegistered(labels))
                        Register(labels);
                }else{
                    // clean stale Vimera keys (portable path moved etc.)
                    bool hasParent = false;
                    try{
                        string[] roots = CtxRoots();
                        for (int r = 0; r < roots.Length && !hasParent; r++){
                            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(roots[r], false)){
                                hasParent = (k != null);
                            }
                        }
                        if (!hasParent){
                            string[] legacy = LegacyCtxRoots();
                            for (int i = 0; i < legacy.Length && !hasParent; i++){
                                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(legacy[i], false)){
                                    hasParent = (k != null);
                                }
                            }
                        }
                    }catch{ }
                    if (hasParent)
                        Unregister();
                }
            }catch{ }
        }
        // SINGLE INSTANCE PIPE
        // ======================================================================================================
        public static bool ForwardToFirstInstance(int hashMode, List<string> files){
            try{
                List<string> safe = files ?? new List<string>();
                using (NamedPipeClientStream client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out)){
                    try{ client.Connect(2500); }
                    catch{ return false; }
                    using (BinaryWriter writer = new BinaryWriter(client, Encoding.UTF8)){
                        writer.Write(hashMode);
                        writer.Write(safe.Count);
                        for (int i = 0; i < safe.Count; i++)
                            writer.Write(Convert.ToString(safe[i]));
                        writer.Flush();
                    }
                }
                return true;
            }catch{ return false; }
        }
    }
}
