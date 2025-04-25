using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UdsTool.Models;
using UdsTool.Services;
using System.IO;
using UdsTool.Commands;
using System.ComponentModel;

namespace UdsTool.ViewModels
{
    public class XmlEditorViewModel : BaseViewModel
    {
        private readonly IXmlService _xmlService;
        private UdsXmlConfig _currentConfig;
        private string _statusMessage;
        private bool _isModified;
        private UdsSid _selectedSid;
        private UdsSubfunction _selectedSubfunction;
        private UdsDid _selectedDid;
        private UdsDataItem _selectedDataItem;
        private object _selectedTreeItem; // 추가: 트리에서 선택된 항목
        private object _selectedItem;

        public XmlEditorViewModel(IXmlService xmlService)
        {
            _xmlService = xmlService;

            // 명령어 초기화
            NewConfigCommand = new RelayCommand(_ => CreateNewConfig());
            OpenConfigCommand = new RelayCommand(_ => OpenConfigAsync());
            SaveConfigCommand = new RelayCommand(_ => SaveConfigAsync(), _ => IsModified);
            SaveAsConfigCommand = new RelayCommand(_ => SaveAsConfigAsync());

            AddSidCommand = new RelayCommand(_ => AddSid());
            AddSubfunctionCommand = new RelayCommand(_ => AddSubfunction(), _ => SelectedSid != null);
            AddDidCommand = new RelayCommand(_ => AddDid(), _ => SelectedSubfunction != null);
            AddDataItemCommand = new RelayCommand(_ => AddDataItem(), _ => SelectedDid != null);

            DeleteSidCommand = new RelayCommand(_ => DeleteSid(), _ => SelectedSid != null);
            DeleteSubfunctionCommand = new RelayCommand(_ => DeleteSubfunction(), _ => SelectedSubfunction != null);
            DeleteDidCommand = new RelayCommand(_ => DeleteDid(), _ => SelectedDid != null);
            DeleteDataItemCommand = new RelayCommand(_ => DeleteDataItem(), _ => SelectedDataItem != null);

            // 통합 삭제 명령
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedItem != null);

            // 통합 저장 명령 추가
            SaveChangesCommand = new RelayCommand(_ => SaveChanges(), _ => IsModified);

            // PropertyChanged 이벤트 처리로 수정 상태 감지
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedSid) ||
                    e.PropertyName == nameof(SelectedSubfunction) ||
                    e.PropertyName == nameof(SelectedDid) ||
                    e.PropertyName == nameof(SelectedDataItem))
                {
                    // 이미 수정된 경우 또는 설정이 로드되지 않은 경우는 무시
                    if (IsModified || _currentConfig?.FilePath == null) return;

                    // 선택된 항목이 변경되면 수정 트래킹은 하지 않음
                    return;
                }

                if (e.PropertyName != nameof(IsModified) &&
                    e.PropertyName != nameof(StatusMessage) &&
                    e.PropertyName != nameof(Services) &&
                    e.PropertyName != nameof(SelectedItem))
                {
                    // 일반 속성이 변경되면 수정 상태로 설정
                    IsModified = true;
                }
            };

            // 기본 설정 생성
            CreateNewConfig();
        }

        public ObservableCollection<UdsSid> Services => new ObservableCollection<UdsSid>(_currentConfig?.Services ?? new System.Collections.Generic.List<UdsSid>());

        // 속성 정의
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    UpdateSelectedItems();
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // 트리에서 선택된 항목 속성 추가
        public object SelectedTreeItem
        {
            get => _selectedTreeItem;
            set
            {
                if (SetProperty(ref _selectedTreeItem, value))
                {
                    UpdateSelectedItems(value);
                }
            }
        }

        public UdsSid SelectedSid
        {
            get => _selectedSid;
            set
            {
                if (SetProperty(ref _selectedSid, value))
                {
                    ((RelayCommand)AddSubfunctionCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteSidCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public UdsSubfunction SelectedSubfunction
        {
            get => _selectedSubfunction;
            set
            {
                if (SetProperty(ref _selectedSubfunction, value))
                {
                    ((RelayCommand)AddDidCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteSubfunctionCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public UdsDid SelectedDid
        {
            get => _selectedDid;
            set
            {
                if (SetProperty(ref _selectedDid, value))
                {
                    ((RelayCommand)AddDataItemCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteDidCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public UdsDataItem SelectedDataItem
        {
            get => _selectedDataItem;
            set
            {
                if (SetProperty(ref _selectedDataItem, value))
                {
                    ((RelayCommand)DeleteDataItemCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (SetProperty(ref _isModified, value))
                {
                    ((RelayCommand)SaveConfigCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand NewConfigCommand { get; }
        public ICommand OpenConfigCommand { get; }
        public ICommand SaveConfigCommand { get; }
        public ICommand SaveAsConfigCommand { get; }

        public ICommand AddSidCommand { get; }
        public ICommand AddSubfunctionCommand { get; }
        public ICommand AddDidCommand { get; }
        public ICommand AddDataItemCommand { get; }

        public ICommand DeleteSidCommand { get; }
        public ICommand DeleteSubfunctionCommand { get; }
        public ICommand DeleteDidCommand { get; }
        public ICommand DeleteDataItemCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveChangesCommand { get; }

        // 통합 저장 메서드 추가
        private void SaveChanges()
        {
            if (_currentConfig == null || string.IsNullOrEmpty(_currentConfig.FilePath))
            {
                SaveAsConfigAsync();
                return;
            }

            SaveConfigAsync();
        }

        private void CreateNewConfig()
        {
            _currentConfig = _xmlService.CreateDefaultConfiguration();
            IsModified = true;
            StatusMessage = "새 설정 파일이 생성되었습니다.";
            OnPropertyChanged(nameof(Services));
        }

        private async void OpenConfigAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML 파일 (*.xml)|*.xml|모든 파일 (*.*)|*.*",
                Title = "UDS 설정 파일 열기"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _currentConfig = await _xmlService.LoadConfigurationAsync(openFileDialog.FileName);
                    IsModified = false;
                    StatusMessage = $"파일을 성공적으로 불러왔습니다: {openFileDialog.FileName}";
                    OnPropertyChanged(nameof(Services));
                }
                catch (Exception ex)
                {
                    StatusMessage = $"파일을 불러오는 중 오류가 발생했습니다: {ex.Message}";
                }
            }
        }

        private async void SaveConfigAsync()
        {
            if (string.IsNullOrEmpty(_currentConfig.FilePath))
            {
                SaveAsConfigAsync();
                return;
            }

            try
            {
                await _xmlService.SaveConfigurationAsync(_currentConfig, _currentConfig.FilePath);
                IsModified = false;
                StatusMessage = $"파일을 성공적으로 저장했습니다: {_currentConfig.FilePath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"파일을 저장하는 중 오류가 발생했습니다: {ex.Message}";
            }
        }

        private async void SaveAsConfigAsync()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML 파일 (*.xml)|*.xml|모든 파일 (*.*)|*.*",
                Title = "UDS 설정 파일 저장",
                FileName = Path.GetFileName(_currentConfig?.FilePath) ?? "UdsConfig.xml"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _xmlService.SaveConfigurationAsync(_currentConfig, saveFileDialog.FileName);
                    _currentConfig.FilePath = saveFileDialog.FileName;
                    IsModified = false;
                    StatusMessage = $"파일을 성공적으로 저장했습니다: {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"파일을 저장하는 중 오류가 발생했습니다: {ex.Message}";
                }
            }
        }

        private void AddSid()
        {
            var newSid = new UdsSid
            {
                Id = $"0x{_currentConfig.Services.Count + 1:X2}",
                Name = $"Service_{_currentConfig.Services.Count + 1}",
                Description = "새로운 서비스"
            };

            _currentConfig.Services.Add(newSid);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedSid = newSid;
            StatusMessage = $"새 SID가 추가되었습니다: {newSid.Name}";
        }

        private void AddSubfunction()
        {
            if (SelectedSid == null) return;

            var newSubfunction = new UdsSubfunction
            {
                Id = $"0x{SelectedSid.Subfunctions.Count + 1:X2}",
                Name = $"Subfunction_{SelectedSid.Subfunctions.Count + 1}",
                Description = "새로운 서브펑션"
            };

            SelectedSid.Subfunctions.Add(newSubfunction);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedSubfunction = newSubfunction;
            StatusMessage = $"새 서브펑션이 추가되었습니다: {newSubfunction.Name}";
        }

        private void AddDid()
        {
            if (SelectedSubfunction == null) return;

            var newDid = new UdsDid
            {
                Id = $"0xF{SelectedSubfunction.Dids.Count + 1:X3}",
                Name = $"DID_{SelectedSubfunction.Dids.Count + 1}",
                Description = "새로운 데이터 ID"
            };

            SelectedSubfunction.Dids.Add(newDid);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedDid = newDid;
            StatusMessage = $"새 DID가 추가되었습니다: {newDid.Name}";
        }

        private void AddDataItem()
        {
            if (SelectedDid == null) return;

            var newDataItem = new UdsDataItem
            {
                Id = $"Data_{SelectedDid.DataItems.Count + 1}",
                Name = $"DataItem_{SelectedDid.DataItems.Count + 1}",
                Description = "새로운 데이터 항목",
                Length = 1,
                Unit = "byte"
            };

            SelectedDid.DataItems.Add(newDataItem);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedDataItem = newDataItem;
            StatusMessage = $"새 데이터 항목이 추가되었습니다: {newDataItem.Name}";
        }
        // 선택된 항목에 따라 SelectedSid, SelectedSubfunction 등의 속성을 업데이트
        private void UpdateSelectedItems(object selectedItem)
        {
            // 모든 선택 항목 초기화
            if (selectedItem == null)
            {
                SelectedSid = null;
                SelectedSubfunction = null;
                SelectedDid = null;
                SelectedDataItem = null;
                return;
            }

            if (selectedItem is UdsSid sid)
            {
                SelectedSid = sid;
                SelectedSubfunction = null;
                SelectedDid = null;
                SelectedDataItem = null;
            }
            else if (selectedItem is UdsSubfunction subfunction)
            {
                // 부모 SID 찾기
                SelectedSid = _currentConfig.Services.FirstOrDefault(s => s.Subfunctions.Contains(subfunction));
                SelectedSubfunction = subfunction;
                SelectedDid = null;
                SelectedDataItem = null;
            }
            else if (selectedItem is UdsDid did)
            {
                // 부모 SID와 Subfunction 찾기
                foreach (var sid1 in _currentConfig.Services)
                {
                    var subfunction1 = sid1.Subfunctions.FirstOrDefault(sf => sf.Dids.Contains(did));
                    if (subfunction1 != null)
                    {
                        SelectedSid = sid1;
                        SelectedSubfunction = subfunction1;
                        break;
                    }
                }

                SelectedDid = did;
                SelectedDataItem = null;
            }
            else if (selectedItem is UdsDataItem dataItem)
            {
                // 부모 SID, Subfunction, DID 찾기
                foreach (var sid2 in _currentConfig.Services)
                {
                    foreach (var subfunction2 in sid2.Subfunctions)
                    {
                        var did2 = subfunction2.Dids.FirstOrDefault(d => d.DataItems.Contains(dataItem));
                        if (did2 != null)
                        {
                            SelectedSid = sid2;
                            SelectedSubfunction = subfunction2;
                            SelectedDid = did2;
                            break;
                        }
                    }
                }

                SelectedDataItem = dataItem;
            }
        }

        private void DeleteSid()
        {
            if (SelectedSid == null) return;

            _currentConfig.Services.Remove(SelectedSid);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedSid = null;
            StatusMessage = "SID가 삭제되었습니다.";
        }

        private void DeleteSubfunction()
        {
            if (SelectedSid == null || SelectedSubfunction == null) return;

            SelectedSid.Subfunctions.Remove(SelectedSubfunction);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedSubfunction = null;
            StatusMessage = "서브펑션이 삭제되었습니다.";
        }

        private void DeleteDid()
        {
            if (SelectedSubfunction == null || SelectedDid == null) return;

            SelectedSubfunction.Dids.Remove(SelectedDid);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedDid = null;
            StatusMessage = "DID가 삭제되었습니다.";
        }

        private void DeleteDataItem()
        {
            if (SelectedDid == null || SelectedDataItem == null) return;

            SelectedDid.DataItems.Remove(SelectedDataItem);
            IsModified = true;
            OnPropertyChanged(nameof(Services));
            SelectedDataItem = null;
            StatusMessage = "데이터 항목이 삭제되었습니다.";
        }
        // 삭제 가능 여부 확인
        private bool CanDelete()
        {
            return SelectedSid != null;
        }

        // 통합 삭제 메서드
        private void Delete()
        {
            if (SelectedDataItem != null)
                DeleteDataItem();
            else if (SelectedDid != null)
                DeleteDid();
            else if (SelectedSubfunction != null)
                DeleteSubfunction();
            else if (SelectedSid != null)
                DeleteSid();
        }
        // TextBox에서 PropertyChanged 이벤트를 감지하기 위한 추가 코드
        private void SetupPropertyChangedHandler(object obj)
        {
            if (obj is INotifyPropertyChanged notifyObj)
            {
                notifyObj.PropertyChanged += (s, e) =>
                {
                    if (_currentConfig?.FilePath != null)
                    {
                        IsModified = true;
                    }
                };
            }
        }
    }
}
