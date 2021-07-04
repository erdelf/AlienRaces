namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Verse;

    public class Scribe_NestedCollections
    {
        public static void Look<K, V>(ref Dictionary<K, HashSet<V>> dict, string label, LookMode keyLookMode, LookMode valueLookMode, bool forceSave = false)
        {
            List<K>          keysWorkingList   = null;
            List<HashSet<V>> valuesWorkingList = null;
            if (Scribe.EnterNode(label))
                try
                {
                    if (Scribe.mode == LoadSaveMode.Saving && dict == null)
                    {
                        Scribe.saver.WriteAttribute("IsNull", "True");
                        return;
                    }

                    if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        XmlAttribute xmlAttribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
                        if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
                        {
                            dict = null;
                        }
                        else
                        {
                            dict = new Dictionary<K, HashSet<V>>();
                        }
                    }

                    if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        keysWorkingList   = new List<K>();
                        valuesWorkingList = new List<HashSet<V>>();
                        if (Scribe.mode == LoadSaveMode.Saving && dict != null)
                        {
                            foreach (KeyValuePair<K, HashSet<V>> item in dict)
                            {
                                keysWorkingList.Add(item.Key);
                                valuesWorkingList.Add(item.Value);
                            }
                        }
                    }

                    if (Scribe.mode == LoadSaveMode.Saving || dict != null)
                    {
                        Scribe_Collections.Look(ref keysWorkingList, "keys", keyLookMode);
                        Look(ref valuesWorkingList, "values", valueLookMode);
                    }

                    if (Scribe.mode == LoadSaveMode.Saving)
                    {
                        if (keysWorkingList != null)
                        {
                            keysWorkingList.Clear();
                            keysWorkingList = null;
                        }

                        if (valuesWorkingList != null)
                        {
                            valuesWorkingList.Clear();
                            valuesWorkingList = null;
                        }
                    }

                    bool flag = keyLookMode == LookMode.Reference || valueLookMode == LookMode.Reference;
                    if (((flag && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (!flag && Scribe.mode == LoadSaveMode.LoadingVars)) && dict != null)
                    {
                        if (keysWorkingList == null)
                        {
                            Log.Error("Cannot fill dictionary because there are no keys. label=" + label);
                        }
                        else if (valuesWorkingList == null)
                        {
                            Log.Error("Cannot fill dictionary because there are no values. label=" + label);
                        }
                        else
                        {
                            if (keysWorkingList.Count != valuesWorkingList.Count)
                            {
                                Log.Error("Keys count does not match the values count while loading a dictionary (maybe keys and values were resolved during different passes?). Some elements will be skipped. keys=" +
                                          keysWorkingList.Count + ", values=" + valuesWorkingList.Count + ", label=" + label);
                            }

                            int num = Math.Min(keysWorkingList.Count, valuesWorkingList.Count);
                            for (int i = 0; i < num; i++)
                            {
                                if (keysWorkingList[i] == null)
                                {
                                    Log.Error(string.Concat("Null key while loading dictionary of ", typeof(K), " and ", typeof(V), ". label=", label));
                                    continue;
                                }

                                try
                                {
                                    dict.Add(keysWorkingList[i], valuesWorkingList[i]);
                                }
                                catch (OutOfMemoryException)
                                {
                                    throw;
                                }
                                catch (Exception ex2)
                                {
                                    Log.Error("Exception in LookDictionary(label=" + label + "): " + ex2);
                                }
                            }
                        }
                    }

                    if (Scribe.mode == LoadSaveMode.PostLoadInit)
                    {
                        keysWorkingList?.Clear();
                        valuesWorkingList?.Clear();
                    }
                }
                finally
                {
                    Scribe.ExitNode();
                }
            else if (Scribe.mode == LoadSaveMode.LoadingVars) dict = null;
        }

        public static void Look<T>(ref List<HashSet<T>> list, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
        {
            if (lookMode == LookMode.Undefined && !Scribe_Universal.TryResolveLookMode(typeof(T), out lookMode))
            {
                Log.Error(string.Concat("LookList call with a list of ", typeof(T), " must have lookMode set explicitly."));
            }
            else if (Scribe.EnterNode(label))
            {
                try
                {
                    switch (Scribe.mode)
                    {
                        case LoadSaveMode.Saving when list == null:
                            Scribe.saver.WriteAttribute("IsNull", "True");
                            return;
                        case LoadSaveMode.Saving:
                        {
                            foreach (HashSet<T> hashSet in list)
                            {
                                HashSet<T> li = hashSet;
                                Scribe_Collections.Look(ref li, "li");
                            }

                            break;
                        }
                        case LoadSaveMode.LoadingVars:
                        {
                            XmlNode      curXmlParent = Scribe.loader.curXmlParent;
                            XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
                            if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
                            {
                                if (lookMode == LookMode.Reference)
                                {
                                    Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, null);
                                }

                                list = null;
                                return;
                            }

                            list = new List<HashSet<T>>(curXmlParent.ChildNodes.Count);

                            foreach (XmlNode childNode in curXmlParent.ChildNodes)
                            {
                                HashSet<T> li = null;
                                Scribe_Collections.Look(ref li, "li");
                                list.Add(li);
                            }

                            break;
                        }
                    }
                }
                finally
                {
                    Scribe.ExitNode();
                }
            }
        }
    }
}