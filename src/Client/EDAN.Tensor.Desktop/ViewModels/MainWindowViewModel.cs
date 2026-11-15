using System.Collections.ObjectModel;
using EDAN.Tensor.Desktop.Infrastructure;

namespace EDAN.Tensor.Desktop.ViewModels;

public sealed class ScreenNavigationItem(string screenId, string title, RegistryScreenViewModel viewModel) : ObservableObject
{
    public string ScreenId { get; } = screenId;
    public string Title { get; } = title;
    public RegistryScreenViewModel ViewModel { get; } = viewModel;
}

public sealed class MainWindowViewModel : ObservableObject
{
    private ScreenNavigationItem? _selectedScreen;
    private string _connectionStatus = "API 연결 전";

    public MainWindowViewModel()
    {
        Session = new DesktopApiSession();
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        Screens =
        [
            new("PLT-TNT-001", "테넌트 관리", new TenantManagementViewModel(Session)),
            new("PLT-ORG-001", "조직 관리", new OrganizationManagementViewModel(Session)),
            new("PLT-PJT-001", "프로젝트 관리", new ProjectManagementViewModel(Session)),
            new("PLT-USR-001", "사용자 초대", new UserInvitationManagementViewModel(Session)),
            new("PLT-MBR-001", "멤버십 관리", new MembershipManagementViewModel(Session)),
            new("PLT-AUTH-001", "권한 정책", new AuthorizationPolicyManagementViewModel(Session)),
            new("PLT-AUD-001", "권한 감사", new AuthorizationAuditManagementViewModel(Session)),
            new("PLT-QTA-001", "저장 용량 할당량", new StorageQuotaManagementViewModel(Session)),
            new("DSET-DSM-001", "데이터셋 관리", new DataSetManagementViewModel(Session)),
            new("DSET-SCHM-001", "데이터셋 스키마", new DataSetSchemaManagementViewModel(Session)),
            new("DSET-STO-001", "데이터셋 저장소", new DataSetStorageManagementViewModel(Session)),
            new("DSET-VLD-001", "데이터셋 검증", new DataSetValidationManagementViewModel(Session)),
            new("WRK-TSK-001", "작업 모니터링", new WorkerTaskMonitoringViewModel(Session)),
            new("WRK-DLQ-001", "실패 작업", new DeadLetterQueueManagementViewModel(Session)),
            new("WRK-XDL-001", "외부 전달", new ExternalDeliveryMonitoringViewModel(Session))
        ];
        SelectedScreen = Screens[0];
    }

    public DesktopApiSession Session { get; }
    public AsyncRelayCommand ConnectCommand { get; }

    public ObservableCollection<ScreenNavigationItem> Screens { get; }

    public ScreenNavigationItem? SelectedScreen
    {
        get => _selectedScreen;
        set
        {
            if (!SetProperty(ref _selectedScreen, value) || value is null)
                return;
            _ = value.ViewModel.RefreshAsync();
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        private set => SetProperty(ref _connectionStatus, value);
    }

    public void MarkConnection(string message) => ConnectionStatus = message;

    private async Task ConnectAsync()
    {
        await Session.ConnectAsync();
        ConnectionStatus = Session.StatusMessage;
        if (SelectedScreen is not null)
            await SelectedScreen.ViewModel.RefreshAsync();
    }
}
