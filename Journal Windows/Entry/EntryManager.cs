using JournalTrace.Entry;
using JournalTrace.Language;
using JournalTrace.Native;
using JournalTrace.View.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JournalTrace.Entry
{
    public class EntryManager
    {
        #region events

        public event EventHandler<float> StatusProgressUpdate;

        public event EventHandler<bool> NextStatusUpdate;

        public event EventHandler WorkEnded;

        protected virtual void OnStatusProgressUpdate()
        {
            StatusProgressUpdate?.Invoke(this, 1f);
        }

        protected virtual void OnEntryAmountUpdate(bool completed)
        {
            NextStatusUpdate?.Invoke(this, completed);
        }

        protected virtual void OnWorkEnded()
        {
            WorkEnded?.Invoke(this, null);
        }

        #endregion events

        private DriveInfo selectedVolume;

        public void ChangeVolume(DriveInfo newVolume)
        {
            this.selectedVolume = newVolume;
        }

        // используется для выбора ячейки в datagrid
        public long SelectedUSN;





        // используется для получения самой старой даты usn, отображается на главной форме
        public long OldestUSN;

        public Win32Api.USN_JOURNAL_DATA usnCurrentJournalState;
        private NtfsUsnJournal usnJournal = null;

        public void BeginScan()
        {
            // очистка
            parentFileReferenceIdentifiers.Clear();
            USNEntries.Clear();
            USNDirectories.Clear();

            usnCurrentJournalState = new Win32Api.USN_JOURNAL_DATA();
            // 1 фаза; получение дескриптора
            try
            {
                usnJournal = new NtfsUsnJournal(selectedVolume);
                OnEntryAmountUpdate(true);
            }
            catch (Exception)
            {
                OnEntryAmountUpdate(false);
                return;
            }

            // 2 фаза; текущее состояние
            Win32Api.USN_JOURNAL_DATA journalState = new Win32Api.USN_JOURNAL_DATA();
            NtfsUsnJournal.UsnJournalReturnCode rtn = usnJournal.GetUsnJournalState(ref journalState);
            if (rtn == NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
            {
                usnCurrentJournalState = journalState;
                OnEntryAmountUpdate(true);
            }
            else
            {
                OnEntryAmountUpdate(false);
                return;
            }

            // 3 фаза; запрос
            uint reasonMask = Win32Api.USN_REASON_DATA_OVERWRITE |
                    Win32Api.USN_REASON_DATA_EXTEND |
                    Win32Api.USN_REASON_NAMED_DATA_OVERWRITE |
                    Win32Api.USN_REASON_NAMED_DATA_TRUNCATION |
                    Win32Api.USN_REASON_FILE_CREATE |
                    Win32Api.USN_REASON_FILE_DELETE |
                    Win32Api.USN_REASON_EA_CHANGE |
                    Win32Api.USN_REASON_SECURITY_CHANGE |
                    Win32Api.USN_REASON_RENAME_OLD_NAME |
                    Win32Api.USN_REASON_RENAME_NEW_NAME |
                    Win32Api.USN_REASON_INDEXABLE_CHANGE |
                    Win32Api.USN_REASON_BASIC_INFO_CHANGE |
                    Win32Api.USN_REASON_HARD_LINK_CHANGE |
                    Win32Api.USN_REASON_COMPRESSION_CHANGE |
                    Win32Api.USN_REASON_ENCRYPTION_CHANGE |
                    Win32Api.USN_REASON_OBJECT_ID_CHANGE |
                    Win32Api.USN_REASON_REPARSE_POINT_CHANGE |
                    Win32Api.USN_REASON_STREAM_CHANGE |
                    Win32Api.USN_REASON_CLOSE;

            OldestUSN = usnCurrentJournalState.FirstUsn;
            NtfsUsnJournal.UsnJournalReturnCode rtnCode = usnJournal.GetUsnJournalEntries(usnCurrentJournalState, reasonMask, out List<Win32Api.UsnEntry> usnEntries, out usnCurrentJournalState);

            if (rtnCode == NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
            {
                OnEntryAmountUpdate(true);

                // 4 фаза
                ResolveIdentifiers(usnEntries);
                OnEntryAmountUpdate(true);

                // 5 фаза
                AddEntries(usnEntries);
                OnEntryAmountUpdate(true);

                OnWorkEnded();
            }
            else
            {
                OnEntryAmountUpdate(false);
                return;
            }
        }

        public IDictionary<long, USNEntry> USNEntries = new Dictionary<long, USNEntry>(); // usn
        public IDictionary<ulong, USNCollection> USNDirectories = new Dictionary<ulong, USNCollection>(); // ссылка на родительский файл
        public IDictionary<ulong, USNCollection> USNFiles = new Dictionary<ulong, USNCollection>(); // ссылка на файл

        private void AddEntries(List<Win32Api.UsnEntry> usnEntries)
        {
            foreach (var entry in usnEntries)
            {
                ulong parentFileReference = entry.ParentFileReferenceNumber;
                ulong fileReference = entry.FileReferenceNumber;
                USNEntries.Add(entry.USN, new USNEntry(entry.USN, entry.Name, entry.FileReferenceNumber, entry.ParentFileReferenceNumber, entry.TimeStamp, entry.Reason));
                // директории
                if (!USNDirectories.TryGetValue(parentFileReference, out USNCollection foundDir))
                {
                    USNDirectories.Add(parentFileReference, new USNCollection(parentFileReference, entry.USN));
                }
                else
                {
                    foundDir.USNList.Add(entry.USN);
                }
                // файлы
                if (!USNFiles.TryGetValue(fileReference, out USNCollection foundFile))
                {
                    USNFiles.Add(fileReference, new USNCollection(fileReference, entry.USN));
                }
                else
                {
                    foundFile.USNList.Add(entry.USN);
                }
            }


            string usnReasonsRaw = LanguageManager.INSTANCE.GetString("usnreasons");
            string[] usnReasonsList = usnReasonsRaw.Split(new string[] { "," }, StringSplitOptions.None);
            foreach (var entry in USNEntries)
            {
                entry.Value.ResolveInfo(usnReasonsList);
            }

            // MessageBox.Show("c");
        }

        public IDictionary<ulong, ResolvableIdentifier> parentFileReferenceIdentifiers = new Dictionary<ulong, ResolvableIdentifier>();
        public int fileReferenceIndetifiersSize = 0;

        private void ResolveIdentifiers(List<Win32Api.UsnEntry> usnEntries)
        {
            // помещаем все id родительских директорий в hashset
            // hashset не принимает дубликаты и работает быстрее обычного списка
            // результат - список с уникальными id
            HashSet<ulong> fileReference = new HashSet<ulong>(), parentFileReference = new HashSet<ulong>();

            foreach (var entry in usnEntries)
            {
                fileReference.Add(entry.FileReferenceNumber);
                parentFileReference.Add(entry.ParentFileReferenceNumber);
            }

            fileReferenceIndetifiersSize = fileReference.Count;

            // заполнение идентификаторов
            foreach (var id in parentFileReference)
            {
                parentFileReferenceIdentifiers.Add(id, new ResolvableIdentifier(id));
            }

            // разрешение идентификаторов в словаре
            foreach (var item in parentFileReferenceIdentifiers)
            {
                item.Value.Resolve();
            }

        }

        // ищет в узлах параметра узел с именем параметра
        // используется когда "ContainsKey" возвращает true и нам нужно получить конкретный узел
        private TreeNode GetNodeOfName(TreeNode nodeToSearch, string name)
        {
            foreach (TreeNode node in nodeToSearch.Nodes)
            {
                if (node.Name.Equals(name))
                {
                    return node;
                }
            }
            return null;
        }

        // дерево не имеет ссылки на id каждой отдельной директории
        // чтобы получить изменения директории, ищем по полному имени
        public List<long> GetChangesOfDirectory(string path)
        {
            USNCollection foundEntry = null;
            foreach (var usndir in USNDirectories)
            {
                if (parentFileReferenceIdentifiers[usndir.Key].ResolvedID.Equals(path))
                {
                    foundEntry = usndir.Value;
                    break;
                }
            }
            if (foundEntry != null)
            {
                return foundEntry.USNList;
            }
            else
            {
                return null;
            }
        }

        public TreeNode[] BakeTree()
        {
            List<TreeNode> rootDirNodes = new List<TreeNode>();
            // создаем дерево для каждой директории для более удобного использования
            foreach (var usndir in USNDirectories)
            {
                // разделяем строку пути по разделителю директорий (обратный слэш) в массив
                // каждый индекс содержит каждую директорию отдельно
                // пример: "C:\Users\Computador\Downloads\" -> "C:", "Users", "Computador", "Downloads"
                // это нужно для проверки существует ли определенная директория в дереве
                string parentFilePath = parentFileReferenceIdentifiers[usndir.Key].ResolvedID;
                string[] individualDirs = parentFilePath.Split('\\');
                if (individualDirs.Length == 2)
                {
                    if (individualDirs[1].Equals(""))
                    {
                        individualDirs = new string[] { individualDirs[0] };
                    }
                }

                TreeNode lastNode = new TreeNode();

                // для каждой отдельной директории нужно обновить дерево (при необходимости)
                // если отдельная директория не существует, создаем соответствующий узел (если это первый индекс, это корневая директория в дереве)
                // если существует, проверяем не является ли индекс цикла последним
                // последний индекс указывает что директория содержит изменения и мы должны отличить её от остальных
                for (int i = 0; i < individualDirs.Length; i++)
                {
                    string individualDir = individualDirs[i];
                    if (i == 0)
                    {
                        // первый индекс, требуется другая логика для размещения корня
                        bool found = false;
                        foreach (var rootDirNode in rootDirNodes)
                        {
                            if (rootDirNode.Name.Equals(individualDir))
                            {
                                lastNode = rootDirNode;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            TreeNode newRootNode = new TreeNode
                            {
                                Name = individualDir,
                                Text = individualDir,
                                ForeColor = ModernTheme.TextSecondary
                            };

                            rootDirNodes.Add(newRootNode);
                            lastNode = newRootNode;
                        }
                    }
                    else
                    {
                        // не первый индекс
                        if (!lastNode.Nodes.ContainsKey(individualDir))
                        {
                            TreeNode newNode = new TreeNode
                            {
                                Name = individualDir,
                                Text = individualDir,
                                ForeColor = ModernTheme.TextSecondary
                            };

                            lastNode.Nodes.Add(newNode);
                            lastNode = newNode;
                        }
                        else
                        {
                            lastNode = GetNodeOfName(lastNode, individualDir);
                        }
                    }

                    // отличаем узел если индекс последний
                    if (i == individualDirs.Length - 1)
                    {
                        lastNode.ForeColor = ModernTheme.TextPrimary;
                    }
                }
            }

            // сортировка по строке чтобы диск всегда появлялся сверху
            rootDirNodes.Sort((x, y) => y.Text.CompareTo(x.Text));

            return rootDirNodes.ToArray();
        }
    }
}