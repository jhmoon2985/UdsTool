using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UdsTool.Core.Base;
using UdsTool.Core.Interfaces;
using UdsTool.Core.Models;

namespace UdsTool.ViewModels
{
    public class XmlEditorViewModel : ViewModelBase
    {
        private readonly IXmlService _xmlService;
        private UdsDatabase _database;
        private UdsElement _selectedElement;
        private string _currentFilePath;
        private bool _isModified;

        public UdsDatabase Database
        {
            get => _database;
            private set => SetProperty(ref _database, value);
        }

        public UdsElement SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }

        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }

        public ICommand NewDatabaseCommand { get; }
        public ICommand LoadDatabaseCommand { get; }
        public ICommand SaveDatabaseCommand { get; }
        public ICommand SaveAsDatabaseCommand { get; }
        public ICommand AddServiceCommand { get; }
        public ICommand AddSubfunctionCommand { get; }
        public ICommand AddDataIdCommand { get; }
        public ICommand AddDataCommand { get; }
        public ICommand DeleteElementCommand { get; }

        public XmlEditorViewModel(IXmlService xmlService)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));

            // Initialize commands
            NewDatabaseCommand = new RelayCommand(_ => NewDatabase());
            LoadDatabaseCommand = new RelayCommand(_ => LoadDatabaseAsync());
            SaveDatabaseCommand = new RelayCommand(_ => SaveDatabaseAsync(), _ => IsModified);
            SaveAsDatabaseCommand = new RelayCommand(_ => SaveDatabaseAsAsync());
            AddServiceCommand = new RelayCommand(_ => AddService());
            AddSubfunctionCommand = new RelayCommand(_ => AddSubfunction(), CanAddSubfunction);
            AddDataIdCommand = new RelayCommand(_ => AddDataId(), CanAddDataId);
            AddDataCommand = new RelayCommand(_ => AddData(), CanAddData);
            DeleteElementCommand = new RelayCommand(_ => DeleteElement(), _ => SelectedElement != null);

            // Initialize with a new database
            NewDatabase();
        }

        private void NewDatabase()
        {
            Database = _xmlService.CreateDefaultDatabase();
            _currentFilePath = null;
            IsModified = false;
            SelectedElement = null;
        }

        private async void LoadDatabaseAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                    Title = "Open UDS Database"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Loading database...";

                    var database = await _xmlService.LoadDatabaseAsync(openFileDialog.FileName);
                    Database = database;
                    _currentFilePath = openFileDialog.FileName;
                    IsModified = false;
                    SelectedElement = null;

                    StatusMessage = "Database loaded successfully.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveDatabaseAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    await SaveDatabaseAsAsync();
                    return;
                }

                IsBusy = true;
                StatusMessage = "Saving database...";

                await _xmlService.SaveDatabaseAsync(_currentFilePath, Database);
                IsModified = false;

                StatusMessage = "Database saved successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveDatabaseAsAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                    Title = "Save UDS Database"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Saving database...";

                    await _xmlService.SaveDatabaseAsync(saveFileDialog.FileName, Database);
                    _currentFilePath = saveFileDialog.FileName;
                    IsModified = false;

                    StatusMessage = "Database saved successfully.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddService()
        {
            var serviceId = new ServiceId
            {
                Name = "NewService",
                Description = "New Service Description",
                Value = 0x00
            };

            Database.ServiceIds.Add(serviceId);
            SelectedElement = serviceId;
            IsModified = true;
        }

        private void AddSubfunction()
        {
            if (SelectedElement is ServiceId serviceId)
            {
                var subfunction = new Subfunction
                {
                    Name = "NewSubfunction",
                    Description = "New Subfunction Description",
                    Value = 0x00
                };

                serviceId.Subfunctions.Add(subfunction);
                SelectedElement = subfunction;
                IsModified = true;
            }
        }

        private bool CanAddSubfunction(object parameter)
        {
            return SelectedElement is ServiceId;
        }

        private void AddDataId()
        {
            if (SelectedElement is ServiceId serviceId)
            {
                var dataId = new DataId
                {
                    Name = "NewDataId",
                    Description = "New Data ID Description",
                    Value = 0x00
                };

                serviceId.DataIds.Add(dataId);
                SelectedElement = dataId;
                IsModified = true;
            }
        }

        private bool CanAddDataId(object parameter)
        {
            return SelectedElement is ServiceId;
        }

        private void AddData()
        {
            if (SelectedElement is Subfunction subfunction)
            {
                var data = new UdsData
                {
                    Name = "NewData",
                    Value = "00 00 00"
                };

                subfunction.Data.Add(data);
                IsModified = true;
            }
            else if (SelectedElement is DataId dataId)
            {
                var data = new UdsData
                {
                    Name = "NewData",
                    Value = "00 00 00"
                };

                dataId.Data.Add(data);
                IsModified = true;
            }
        }

        private bool CanAddData(object parameter)
        {
            return SelectedElement is Subfunction || SelectedElement is DataId;
        }

        private void DeleteElement()
        {
            if (SelectedElement == null)
                return;

            if (SelectedElement is ServiceId serviceId)
            {
                Database.ServiceIds.Remove(serviceId);
                SelectedElement = null;
                IsModified = true;
            }
            else if (SelectedElement is Subfunction subfunction)
            {
                // Find parent service
                foreach (var service in Database.ServiceIds)
                {
                    if (service.Subfunctions.Contains(subfunction))
                    {
                        service.Subfunctions.Remove(subfunction);
                        SelectedElement = service;
                        IsModified = true;
                        break;
                    }
                }
            }
            else if (SelectedElement is DataId dataId)
            {
                // Find parent service
                foreach (var service in Database.ServiceIds)
                {
                    if (service.DataIds.Contains(dataId))
                    {
                        service.DataIds.Remove(dataId);
                        SelectedElement = service;
                        IsModified = true;
                        break;
                    }
                }
            }
            else if (SelectedElement is UdsData data)
            {
                // Find parent subfunction or DID
                foreach (var service in Database.ServiceIds)
                {
                    foreach (var subfunction in service.Subfunctions)
                    {
                        if (subfunction.Data.Contains(data))
                        {
                            subfunction.Data.Remove(data);
                            SelectedElement = subfunction;
                            IsModified = true;
                            return;
                        }
                    }

                    foreach (var did in service.DataIds)
                    {
                        if (did.Data.Contains(data))
                        {
                            did.Data.Remove(data);
                            SelectedElement = did;
                            IsModified = true;
                            return;
                        }
                    }
                }
            }
        }
    }
}