using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UdsTool.Commands;
using UdsTool.Models;
using UdsTool.Services;
using Microsoft.Win32;
using UdsTool.Views;
using System.Linq;
using System;
using System.Collections.Specialized;

namespace UdsTool.ViewModels
{
    public class XmlEditorViewModel : INotifyPropertyChanged
    {
        private readonly IXmlService _xmlService;
        private readonly IDialogService _dialogService;
        private DiagnosticFrame _selectedFrame;
        private DiagnosticFrame _selectedRequestFrame;
        private DiagnosticFrame _selectedResponseFrame;
        private string _xmlContent;
        private bool _isDirty;
        private string _originalXmlContent;

        public XmlEditorViewModel(IXmlService xmlService, IDialogService dialogService)
        {
            _xmlService = xmlService;
            _dialogService = dialogService;

            // 초기화는 생성자에서 한 번만 실행
            if (DiagnosticFrames == null)
            {
                DiagnosticFrames = new ObservableCollection<DiagnosticFrame>();
                DiagnosticFrames.CollectionChanged += DiagnosticFrames_CollectionChanged;
                
                RequestFrames = new ObservableCollection<DiagnosticFrame>();
                ResponseFrames = new ObservableCollection<DiagnosticFrame>();
            }

            // 명령어 초기화
            AddRequestCommand = new RelayCommand(_ => AddFrame(RequestResponseType.Request));
            AddResponseCommand = new RelayCommand(_ => AddFrame(RequestResponseType.Response));
            AddResponseToSelectedCommand = new RelayCommand(_ => AddResponseToSelected(), _ => CanAddResponseToSelected());
            EditFrameCommand = new RelayCommand(_ => EditFrame(), _ => SelectedFrame != null);
            DeleteFrameCommand = new RelayCommand(_ => DeleteFrame(), _ => SelectedFrame != null);
            SaveCommand = new RelayCommand(_ => Save());
            LoadCommand = new RelayCommand(_ => Load());
            MoveUpCommand = new RelayCommand(_ => MoveUp(), _ => CanMoveUp());
            MoveDownCommand = new RelayCommand(_ => MoveDown(), _ => CanMoveDown());

            // 초기 상태 설정
            if (string.IsNullOrEmpty(_xmlContent))
            {
                UpdateXmlView();
                // 초기 XML 내용 저장 (변경 감지용)
                _originalXmlContent = _xmlContent;
                _isDirty = false;
            }
        }

        public ObservableCollection<DiagnosticFrame> DiagnosticFrames { get; set; }
        public ObservableCollection<DiagnosticFrame> RequestFrames { get; set; }
        public ObservableCollection<DiagnosticFrame> ResponseFrames { get; set; }

        public DiagnosticFrame SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
                OnPropertyChanged();
            }
        }

        public DiagnosticFrame SelectedRequestFrame
        {
            get => _selectedRequestFrame;
            set
            {
                _selectedRequestFrame = value;
                if (value != null && (SelectedFrame == null || SelectedFrame.Type != RequestResponseType.Request))
                {
                    SelectedFrame = value;
                }
                OnPropertyChanged();
            }
        }

        public DiagnosticFrame SelectedResponseFrame
        {
            get => _selectedResponseFrame;
            set
            {
                _selectedResponseFrame = value;
                if (value != null && (SelectedFrame == null || SelectedFrame.Type != RequestResponseType.Response))
                {
                    SelectedFrame = value;
                }
                OnPropertyChanged();
            }
        }

        public string XmlContent
        {
            get => _xmlContent;
            set
            {
                _xmlContent = value;
                CheckIfDirty();
                OnPropertyChanged();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddRequestCommand { get; }
        public ICommand AddResponseCommand { get; }
        public ICommand AddResponseToSelectedCommand { get; }
        public ICommand EditFrameCommand { get; }
        public ICommand DeleteFrameCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        private bool CanMoveUp()
        {
            if (SelectedFrame == null) return false;

            if (SelectedFrame.Type == RequestResponseType.Request)
            {
                int index = RequestFrames.IndexOf(SelectedFrame);
                return index > 0;
            }
            else
            {
                int index = ResponseFrames.IndexOf(SelectedFrame);
                return index > 0;
            }
        }

        private bool CanMoveDown()
        {
            if (SelectedFrame == null) return false;

            if (SelectedFrame.Type == RequestResponseType.Request)
            {
                int index = RequestFrames.IndexOf(SelectedFrame);
                return index < RequestFrames.Count - 1;
            }
            else
            {
                int index = ResponseFrames.IndexOf(SelectedFrame);
                return index < ResponseFrames.Count - 1;
            }
        }

        private bool CanAddResponseToSelected()
        {
            return SelectedRequestFrame != null;
        }

        private void MoveUp()
        {
            if (SelectedFrame != null)
            {
                if (SelectedFrame.Type == RequestResponseType.Request)
                {
                    int index = RequestFrames.IndexOf(SelectedFrame);
                    if (index > 0)
                    {
                        // 순서 속성 교환
                        int currentOrder = SelectedFrame.Idx;
                        SelectedFrame.Idx = RequestFrames[index - 1].Idx;
                        RequestFrames[index - 1].Idx = currentOrder;

                        // 컬렉션에서 항목 위치 변경
                        RequestFrames.Move(index, index - 1);
                    }
                }
                else // Response
                {
                    int index = ResponseFrames.IndexOf(SelectedFrame);
                    if (index > 0)
                    {
                        // 순서 속성 교환
                        int currentOrder = SelectedFrame.Idx;
                        SelectedFrame.Idx = ResponseFrames[index - 1].Idx;
                        ResponseFrames[index - 1].Idx = currentOrder;

                        // 컬렉션에서 항목 위치 변경
                        ResponseFrames.Move(index, index - 1);
                    }
                }
                UpdateXmlView();
            }
        }

        private void MoveDown()
        {
            if (SelectedFrame != null)
            {
                if (SelectedFrame.Type == RequestResponseType.Request)
                {
                    int index = RequestFrames.IndexOf(SelectedFrame);
                    if (index < RequestFrames.Count - 1)
                    {
                        // 순서 속성 교환
                        int currentOrder = SelectedFrame.Idx;
                        SelectedFrame.Idx = RequestFrames[index + 1].Idx;
                        RequestFrames[index + 1].Idx = currentOrder;

                        // 컬렉션에서 항목 위치 변경
                        RequestFrames.Move(index, index + 1);
                    }
                }
                else // Response
                {
                    int index = ResponseFrames.IndexOf(SelectedFrame);
                    if (index < ResponseFrames.Count - 1)
                    {
                        // 순서 속성 교환
                        int currentOrder = SelectedFrame.Idx;
                        SelectedFrame.Idx = ResponseFrames[index + 1].Idx;
                        ResponseFrames[index + 1].Idx = currentOrder;

                        // 컬렉션에서 항목 위치 변경
                        ResponseFrames.Move(index, index + 1);
                    }
                }
                UpdateXmlView();
            }
        }

        private void AddFrame(RequestResponseType type)
        {
            var dialogViewModel = new FrameEditDialogViewModel();
            dialogViewModel.Type = type;
            
            // Request 타입인 경우에만 Response 목록 설정
            if (type == RequestResponseType.Request)
            {
                dialogViewModel.AvailableResponses = ResponseFrames.ToDictionary(r => r.Idx, r => r.Name);
            }

            if (_dialogService.ShowDialog(dialogViewModel) == true)
            {
                var newFrame = dialogViewModel.GetFrame();

                // 타입별로 개별 넘버링 부여
                if (type == RequestResponseType.Request)
                {
                    newFrame.Idx = GetNextRequestIdx();
                    
                    // ResponseIdx 설정 - 선택된 값 사용
                    newFrame.ResponseIdx = dialogViewModel.SelectedResponseIdx;
                    
                    RequestFrames.Add(newFrame);
                    SelectedRequestFrame = newFrame;
                }
                else // Response
                {
                    newFrame.Idx = GetNextResponseIdx();
                    ResponseFrames.Add(newFrame);
                    SelectedResponseFrame = newFrame;
                }

                // 전체 프레임 목록에도 추가
                DiagnosticFrames.Add(newFrame);
                SelectedFrame = newFrame;

                UpdateXmlView();
            }
        }

        private void AddResponseToSelected()
        {
            if (SelectedRequestFrame != null)
            {
                var dialogViewModel = new FrameEditDialogViewModel();
                dialogViewModel.Type = RequestResponseType.Response;
                dialogViewModel.SelectedServiceId = SelectedRequestFrame.ServiceId;

                if (_dialogService.ShowDialog(dialogViewModel) == true)
                {
                    var newFrame = dialogViewModel.GetFrame();
                    newFrame.Idx = GetNextResponseIdx();

                    ResponseFrames.Add(newFrame);
                    DiagnosticFrames.Add(newFrame);
                    
                    SelectedResponseFrame = newFrame;
                    SelectedFrame = newFrame;
                    
                    UpdateXmlView();
                }
            }
        }

        // Request 프레임 편집
        private void EditRequestFrame(DiagnosticFrame requestFrame)
        {
            if (requestFrame != null && requestFrame.Type == RequestResponseType.Request)
            {
                var dialogViewModel = new FrameEditDialogViewModel(requestFrame);
                
                // Request 편집 시 Response 목록 제공
                dialogViewModel.AvailableResponses = ResponseFrames.ToDictionary(r => r.Idx, r => r.Name);
                
                // 기존에 연결된 Response가 있으면 선택
                if (requestFrame.ResponseIdx > 0)
                {
                    dialogViewModel.SelectedResponseIdx = requestFrame.ResponseIdx;
                }
                
                if (_dialogService.ShowDialog(dialogViewModel) == true)
                {
                    var updatedFrame = dialogViewModel.GetFrame();
                    
                    // 기존 인덱스 유지
                    updatedFrame.Idx = requestFrame.Idx;
                    
                    // Response Index 설정
                    updatedFrame.ResponseIdx = dialogViewModel.SelectedResponseIdx;
                    
                    // Request 목록에서 업데이트
                    int index = RequestFrames.IndexOf(requestFrame);
                    if (index >= 0)
                    {
                        RequestFrames[index] = updatedFrame;
                    }

                    // 전체 프레임 목록에서도 업데이트
                    int diagIndex = DiagnosticFrames.IndexOf(requestFrame);
                    if (diagIndex >= 0)
                    {
                        DiagnosticFrames[diagIndex] = updatedFrame;
                    }

                    // 선택된 프레임 업데이트
                    SelectedRequestFrame = updatedFrame;
                    SelectedFrame = updatedFrame;

                    UpdateXmlView();
                }
            }
        }
        
        // Response 프레임 편집
        private void EditResponseFrame(DiagnosticFrame responseFrame)
        {
            if (responseFrame != null && responseFrame.Type == RequestResponseType.Response)
            {
                var dialogViewModel = new FrameEditDialogViewModel(responseFrame);
                
                if (_dialogService.ShowDialog(dialogViewModel) == true)
                {
                    var updatedFrame = dialogViewModel.GetFrame();
                    
                    // 기존 인덱스 유지
                    updatedFrame.Idx = responseFrame.Idx;
                    updatedFrame.ResponseIdx = responseFrame.ResponseIdx;

                    // Response 목록에서 업데이트
                    int index = ResponseFrames.IndexOf(responseFrame);
                    if (index >= 0)
                    {
                        ResponseFrames[index] = updatedFrame;
                    }

                    // 전체 프레임 목록에서도 업데이트
                    int diagIndex = DiagnosticFrames.IndexOf(responseFrame);
                    if (diagIndex >= 0)
                    {
                        DiagnosticFrames[diagIndex] = updatedFrame;
                    }

                    // 선택된 프레임 업데이트
                    SelectedResponseFrame = updatedFrame;
                    SelectedFrame = updatedFrame;

                    UpdateXmlView();
                }
            }
        }

        private void EditFrame()
        {
            if (SelectedFrame != null)
            {
                if (SelectedFrame.Type == RequestResponseType.Request)
                {
                    EditRequestFrame(SelectedFrame);
                }
                else
                {
                    EditResponseFrame(SelectedFrame);
                }
            }
        }

        private void DeleteFrame()
        {
            if (SelectedFrame != null)
            {
                // 전체 프레임 목록에서 삭제
                DiagnosticFrames.Remove(SelectedFrame);
                
                if (SelectedFrame.Type == RequestResponseType.Request)
                {
                    RequestFrames.Remove(SelectedFrame);
                    SelectedRequestFrame = null;
                }
                else // Response
                {
                    ResponseFrames.Remove(SelectedFrame);
                    SelectedResponseFrame = null;
                }
                
                SelectedFrame = null;
                
                // 삭제 후 남은 프레임들의 인덱스 재정렬
                ReindexAllFrames();
                
                UpdateXmlView();
            }
        }
        
        // 모든 프레임의 인덱스를 재정렬하는 메서드
        private void ReindexAllFrames()
        {
            int requestIndex = 1;
            int responseIndex = 1;

            // Request 프레임 인덱스 재정렬
            foreach (var frame in RequestFrames)
            {
                frame.Idx = requestIndex++;
            }

            // Response 프레임 인덱스 재정렬
            foreach (var frame in ResponseFrames)
            {
                frame.Idx = responseIndex++;
            }
        }

        private void Save()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // 저장 전 모든 프레임을 평면화하여 직렬화
                _xmlService.SaveToFile(saveFileDialog.FileName, DiagnosticFrames);

                // 저장 후 변경 상태 초기화
                _originalXmlContent = XmlContent;
                IsDirty = false;
            }
        }

        // 외부에서 호출 가능한 저장 확인 메서드
        public bool PromptSaveIfDirty()
        {
            if (IsDirty)
            {
                var result = System.Windows.MessageBox.Show(
                    "변경된 내용이 있습니다. 저장하시겠습니까?",
                    "저장 확인",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    Save();
                    return true;
                }
                else if (result == System.Windows.MessageBoxResult.No)
                {
                    // 저장하지 않고 계속 진행
                    IsDirty = false;
                    return true;
                }
                else
                {
                    // 작업 취소
                    return false;
                }
            }

            return true; // 변경사항 없음, 계속 진행
        }

        // INavigationAware 인터페이스 구현
        public bool CanNavigateFrom()
        {
            return PromptSaveIfDirty();
        }

        private void Load()
        {
            // 변경사항이 있으면 저장 확인
            if (IsDirty)
            {
                var result = System.Windows.MessageBox.Show(
                    "변경된 내용이 있습니다. 저장하시겠습니까?",
                    "저장 확인",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    Save();
                }
                else if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    return; // 로드 취소
                }
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var loadedFrames = _xmlService.LoadFromFile(openFileDialog.FileName);
                    // 분리된 Request/Response 목록에 로드
                    SeparateRequestAndResponse(loadedFrames);
                    UpdateXmlView();

                    // 로드 후 변경 상태 초기화
                    _originalXmlContent = XmlContent;
                    IsDirty = false;
                }
                catch (Exception ex)
                {
                    // 오류 처리
                    XmlContent = $"Error loading file: {ex.Message}";
                }
            }
        }

        // 평면화된 프레임 목록을 Request와 Response로 분리
        private void SeparateRequestAndResponse(ObservableCollection<DiagnosticFrame> loadedFrames)
        {
            // 기존 목록 초기화
            DiagnosticFrames.Clear();
            RequestFrames.Clear();
            ResponseFrames.Clear();

            // 모든 프레임을 DiagnosticFrames에 추가
            foreach (var frame in loadedFrames)
            {
                DiagnosticFrames.Add(frame);

                // 타입별로 분리
                if (frame.Type == RequestResponseType.Request)
                {
                    RequestFrames.Add(frame);
                }
                else
                {
                    ResponseFrames.Add(frame);
                }
            }
        }

        private void UpdateXmlView()
        {
            try
            {
                XmlContent = _xmlService.SerializeToXml(DiagnosticFrames);
            }
            catch (Exception ex)
            {
                // 오류 처리
                XmlContent = $"Error updating XML view: {ex.Message}";
            }
        }

        // 다음 사용 가능한 Request Idx 값 가져오기
        private int GetNextRequestIdx()
        {
            int maxIdx = 0;

            // Request 프레임에서 최대 Idx 찾기
            foreach (var frame in RequestFrames)
            {
                maxIdx = Math.Max(maxIdx, frame.Idx);
            }

            return maxIdx + 1;
        }

        // 다음 사용 가능한 Response Idx 값 가져오기
        private int GetNextResponseIdx()
        {
            int maxIdx = 0;

            // Response 프레임에서 최대 Idx 찾기
            foreach (var frame in ResponseFrames)
            {
                maxIdx = Math.Max(maxIdx, frame.Idx);
            }

            return maxIdx + 1;
        }

        // 변경사항 확인 메서드
        private void CheckIfDirty()
        {
            IsDirty = _originalXmlContent != XmlContent;
        }

        // 컬렉션 변경 이벤트 핸들러
        private void DiagnosticFrames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 컬렉션 변경 시 IsDirty 플래그 설정
            IsDirty = true;

            // 새로 추가된 항목의 변경 알림 구독
            if (e.NewItems != null)
            {
                foreach (DiagnosticFrame item in e.NewItems)
                {
                    SubscribeToFrameChanges(item);
                }
            }
        }

        // 프레임의 속성 변경 이벤트 구독
        private void SubscribeToFrameChanges(DiagnosticFrame frame)
        {
            if (frame == null) return;
            frame.PropertyChanged += Frame_PropertyChanged;
        }

        // 프레임 속성 변경 이벤트 핸들러
        private void Frame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}