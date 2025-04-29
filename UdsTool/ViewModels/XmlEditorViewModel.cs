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

        public DiagnosticFrame SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
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

            // 부모 프레임인 경우 (최상위 레벨)
            if (IsTopLevelFrame(SelectedFrame))
            {
                int index = DiagnosticFrames.IndexOf(SelectedFrame);
                return index > 0;
            }
            // 자식 프레임인 경우
            else
            {
                var parent = FindParentFrame(SelectedFrame);
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(SelectedFrame);
                    return index > 0;
                }
            }
            return false;
        }

        private bool CanMoveDown()
        {
            if (SelectedFrame == null) return false;

            // 부모 프레임인 경우 (최상위 레벨)
            if (IsTopLevelFrame(SelectedFrame))
            {
                int index = DiagnosticFrames.IndexOf(SelectedFrame);
                return index < DiagnosticFrames.Count - 1;
            }
            // 자식 프레임인 경우
            else
            {
                var parent = FindParentFrame(SelectedFrame);
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(SelectedFrame);
                    return index < parent.Children.Count - 1;
                }
            }
            return false;
        }

        private bool CanAddResponseToSelected()
        {
            return SelectedFrame != null && SelectedFrame.Type == RequestResponseType.Request;
        }

        private void MoveUp()
        {
            if (SelectedFrame != null)
            {
                // 부모 프레임인 경우 (최상위 레벨)
                if (IsTopLevelFrame(SelectedFrame))
                {
                    int index = DiagnosticFrames.IndexOf(SelectedFrame);
                    if (index > 0)
                    {
                        // 순서 속성 교환
                        int currentOrder = SelectedFrame.Idx;
                        SelectedFrame.Idx = DiagnosticFrames[index - 1].Idx;
                        DiagnosticFrames[index - 1].Idx = currentOrder;

                        // 컬렉션에서 항목 위치 변경
                        DiagnosticFrames.Move(index, index - 1);
                    }
                }
                // 자식 프레임인 경우
                else
                {
                    var parent = FindParentFrame(SelectedFrame);
                    if (parent != null)
                    {
                        int index = parent.Children.IndexOf(SelectedFrame);
                        if (index > 0)
                        {
                            // 순서 속성 교환
                            int currentOrder = SelectedFrame.Idx;
                            SelectedFrame.Idx = parent.Children[index - 1].Idx;
                            parent.Children[index - 1].Idx = currentOrder;

                            // 컬렉션에서 항목 위치 변경
                            parent.Children.Move(index, index - 1);
                        }
                    }
                }
                UpdateXmlView();
            }
        }

        private void MoveDown()
        {
            if (SelectedFrame != null)
            {
                // 부모 프레임인 경우 (최상위 레벨)
                if (IsTopLevelFrame(SelectedFrame))
                {
                    int index = DiagnosticFrames.IndexOf(SelectedFrame);
                    if (index < DiagnosticFrames.Count - 1)
                    {
                        // 순서 속성 교환
                        int currentOrder = SelectedFrame.Idx;
                        SelectedFrame.Idx = DiagnosticFrames[index + 1].Idx;
                        DiagnosticFrames[index + 1].Idx = currentOrder;

                        // 컬렉션에서 항목 위치 변경
                        DiagnosticFrames.Move(index, index + 1);
                    }
                }
                // 자식 프레임인 경우
                else
                {
                    var parent = FindParentFrame(SelectedFrame);
                    if (parent != null)
                    {
                        int index = parent.Children.IndexOf(SelectedFrame);
                        if (index < parent.Children.Count - 1)
                        {
                            // 순서 속성 교환
                            int currentOrder = SelectedFrame.Idx;
                            SelectedFrame.Idx = parent.Children[index + 1].Idx;
                            parent.Children[index + 1].Idx = currentOrder;

                            // 컬렉션에서 항목 위치 변경
                            parent.Children.Move(index, index + 1);
                        }
                    }
                }
                UpdateXmlView();
            }
        }

        private void AddFrame(RequestResponseType type)
        {
            var dialogViewModel = new FrameEditDialogViewModel();
            dialogViewModel.Type = type;

            if (_dialogService.ShowDialog(dialogViewModel) == true)
            {
                var newFrame = dialogViewModel.GetFrame();

                // 새 항목에 순서 부여
                newFrame.Idx = GetNextIdx();

                // 관계 설정 - Request는 항상 최상위에 추가
                if (type == RequestResponseType.Request)
                {
                    DiagnosticFrames.Add(newFrame);
                    SelectedFrame = newFrame;
                }
                // Response는 선택된 Request의 자식으로 추가
                else
                {
                    if (SelectedFrame != null && SelectedFrame.Type == RequestResponseType.Request)
                    {
                        SelectedFrame.Children.Add(newFrame);
                    }
                    else
                    {
                        // 선택된 Request가 없으면 최상위에 추가
                        DiagnosticFrames.Add(newFrame);
                    }
                    SelectedFrame = newFrame;
                }

                UpdateXmlView();
            }
        }

        private void AddResponseToSelected()
        {
            if (SelectedFrame != null && SelectedFrame.Type == RequestResponseType.Request)
            {
                var dialogViewModel = new FrameEditDialogViewModel();
                dialogViewModel.Type = RequestResponseType.Response;
                dialogViewModel.SelectedServiceId = SelectedFrame.ServiceId;

                if (_dialogService.ShowDialog(dialogViewModel) == true)
                {
                    var newFrame = dialogViewModel.GetFrame();
                    newFrame.Idx = GetNextIdx();

                    SelectedFrame.Children.Add(newFrame);
                    SelectedFrame = newFrame;
                    UpdateXmlView();
                }
            }
        }

        private void EditFrame()
        {
            if (SelectedFrame != null)
            {
                var dialogViewModel = new FrameEditDialogViewModel(SelectedFrame);

                if (_dialogService.ShowDialog(dialogViewModel) == true)
                {
                    var updatedFrame = dialogViewModel.GetFrame();

                    // 부모 프레임인 경우 (최상위 레벨)
                    if (IsTopLevelFrame(SelectedFrame))
                    {
                        int index = DiagnosticFrames.IndexOf(SelectedFrame);
                        if (index >= 0)
                        {
                            // 기존 자식 프레임 보존
                            updatedFrame.Children = SelectedFrame.Children;
                            DiagnosticFrames[index] = updatedFrame;
                            SelectedFrame = updatedFrame;
                        }
                    }
                    // 자식 프레임인 경우
                    else
                    {
                        var parent = FindParentFrame(SelectedFrame);
                        if (parent != null)
                        {
                            int index = parent.Children.IndexOf(SelectedFrame);
                            if (index >= 0)
                            {
                                parent.Children[index] = updatedFrame;
                                SelectedFrame = updatedFrame;
                            }
                        }
                    }

                    UpdateXmlView();
                }
            }
        }

        private void DeleteFrame()
        {
            if (SelectedFrame != null)
            {
                // 부모 프레임인 경우 (최상위 레벨)
                if (IsTopLevelFrame(SelectedFrame))
                {
                    DiagnosticFrames.Remove(SelectedFrame);
                }
                // 자식 프레임인 경우
                else
                {
                    var parent = FindParentFrame(SelectedFrame);
                    if (parent != null)
                    {
                        parent.Children.Remove(SelectedFrame);
                    }
                }
                // 삭제 후 남은 프레임들의 인덱스 재정렬
                ReindexAllFrames();

                UpdateXmlView();
            }
        }

        // 모든 프레임의 인덱스를 재정렬하는 메서드
        private void ReindexAllFrames()
        {
            int index = 1;

            // 먼저 최상위 프레임 인덱스 재정렬
            foreach (var frame in DiagnosticFrames)
            {
                frame.Idx = index++;

                // 각 최상위 프레임의 자식 프레임 인덱스 재정렬
                foreach (var child in frame.Children)
                {
                    child.Idx = index++;
                }
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
                // 저장 전 모든 프레임 (부모 및 자식)을 평면화하여 직렬화
                var allFrames = FlattenFrames();
                _xmlService.SaveToFile(saveFileDialog.FileName, allFrames);

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
                    RestructureFrames(loadedFrames);
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

        private void UpdateXmlView()
        {
            try
            {
                // 모든 프레임 (부모 및 자식)을 평면화하여 XML 생성
                var allFrames = FlattenFrames();
                XmlContent = _xmlService.SerializeToXml(allFrames);
            }
            catch (Exception ex)
            {
                // 오류 처리
                XmlContent = $"Error updating XML view: {ex.Message}";
            }
        }

        // 부모-자식 관계를 평면화하여 모든 프레임을 하나의 컬렉션으로 변환
        private ObservableCollection<DiagnosticFrame> FlattenFrames()
        {
            var result = new ObservableCollection<DiagnosticFrame>();

            foreach (var frame in DiagnosticFrames)
            {
                result.Add(frame);
                foreach (var child in frame.Children)
                {
                    result.Add(child);
                }
            }

            return result;
        }

        // 로드된 평면 프레임 컬렉션을 부모-자식 관계로 재구성
        private void RestructureFrames(ObservableCollection<DiagnosticFrame> flatFrames)
        {
            DiagnosticFrames.Clear();

            // 먼저 Request 프레임을 찾아 상위 컬렉션에 추가
            var requestFrames = flatFrames.Where(f => f.Type == RequestResponseType.Request).ToList();
            foreach (var request in requestFrames)
            {
                request.Children.Clear();
                DiagnosticFrames.Add(request);
            }

            // 그런 다음 Response 프레임을 찾아 적절한 Request의 자식으로 추가
            var responseFrames = flatFrames.Where(f => f.Type == RequestResponseType.Response).ToList();

            foreach (var response in responseFrames)
            {
                // 같은 ServiceId를 가진 Request 찾기
                var matchingRequest = DiagnosticFrames.FirstOrDefault(r => r.ServiceId == response.ServiceId);

                if (matchingRequest != null)
                {
                    matchingRequest.Children.Add(response);
                }
                else
                {
                    // 매칭되는 Request가 없으면 최상위에 추가
                    DiagnosticFrames.Add(response);
                }
            }
        }

        // 선택된 프레임이 최상위 프레임인지 확인
        private bool IsTopLevelFrame(DiagnosticFrame frame)
        {
            return DiagnosticFrames.Contains(frame);
        }

        // 자식 프레임의 부모 프레임 찾기
        private DiagnosticFrame FindParentFrame(DiagnosticFrame childFrame)
        {
            foreach (var frame in DiagnosticFrames)
            {
                if (frame.Children.Contains(childFrame))
                {
                    return frame;
                }
            }
            return null;
        }

        // 다음 사용 가능한 Idx 값 가져오기
        private int GetNextIdx()
        {
            int maxIdx = 0;

            // 모든 프레임(부모 및 자식)에서 최대 Idx 찾기
            foreach (var frame in DiagnosticFrames)
            {
                maxIdx = Math.Max(maxIdx, frame.Idx);

                foreach (var child in frame.Children)
                {
                    maxIdx = Math.Max(maxIdx, child.Idx);
                }
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

            // 자식 프레임도 구독
            foreach (var child in frame.Children)
            {
                SubscribeToFrameChanges(child);
            }

            // 자식 컬렉션 변경 이벤트 구독
            frame.Children.CollectionChanged += DiagnosticFrames_CollectionChanged;
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