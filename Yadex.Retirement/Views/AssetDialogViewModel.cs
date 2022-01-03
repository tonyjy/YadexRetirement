using Prism.Mvvm;

namespace Yadex.Retirement.Views;

public class AssetDialogViewModel : BindableBase
{
    public AssetDialogViewModel(MainWindowViewModel parent, Asset asset = null)
    {
        Parent = Guard.NotNull(nameof(parent), parent);
        GetAssetNameList();
        GetAssetTypeList();

        InitValues(asset);
    }

    public MainWindowViewModel Parent { get; }

    public bool IsNew

    {
        get => _isNew;
        set
        {
            _isNew = value;
            ActionButtonContent = IsNew ? "Create" : "Update";
        }
    }

    private void InitValues(Asset asset)
    {
        if (asset == null)
        {
            IsNew = true;
            AssetId = Guid.NewGuid();
            AssetName = string.Empty;
            AssetDate = DateTime.Today;
            AssetAmount = 0m;
            AssetType = AssetTypes.Cash;
            return;
        }

        IsNew = false;
        AssetId = asset.AssetId;
        AssetName = asset.AssetName;
        AssetDate = asset.AssetDate;
        AssetAmount = asset.AssetAmount;
        AssetType = asset.AssetType;
    }

    private void GetAssetNameList()
    {
        AssetNameList = new ObservableCollection<string>(Parent
            .AllAssets
            .Select(x => x.AssetName)
            .Distinct()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .OrderBy(x => x));
    }

    private void GetAssetTypeList()
    {
        AssetTypeList = new ObservableCollection<string>(new[]
        {
                AssetTypes.Cash,
                AssetTypes.Fixed,
                AssetTypes.Retirement401K,
                AssetTypes.RetirementRrsp,
                AssetTypes.RetirementPension
            });
    }

    #region Bindings

    public string ActionButtonContent
    {
        get => _actionButtonContent;
        set
        {
            _actionButtonContent = value;
            RaisePropertyChanged();
        }
    }

    private string _actionButtonContent;

    public string AssetIdText
    {
        get => _assetIdText;
        set
        {
            _assetIdText = value;
            RaisePropertyChanged();
        }
    }

    private string _assetIdText;

    public Guid AssetId
    {
        get => _assetId;
        set
        {
            _assetId = value;
            RaisePropertyChanged();

            AssetIdText = IsNew ? "NEW" : AssetId.ToString();
        }
    }

    private Guid _assetId;

    public string AssetName
    {
        get => _assetName;
        set
        {
            _assetName = value;
            RaisePropertyChanged();
        }
    }

    private string _assetName;

    public DateTime AssetDate
    {
        get => _assetDate;
        set
        {
            _assetDate = value;
            RaisePropertyChanged();
        }
    }

    private DateTime _assetDate;

    public decimal AssetAmount
    {
        get => _assetAmount;
        set
        {
            _assetAmount = value;
            RaisePropertyChanged();
        }
    }

    private decimal _assetAmount;

    public ObservableCollection<string> AssetNameList
    {
        get => _assetNameList;
        set
        {
            _assetNameList = value;
            RaisePropertyChanged();
        }
    }

    private ObservableCollection<string> _assetNameList;

    public ObservableCollection<string> AssetTypeList
    {
        get => _assetTypeList;
        set
        {
            _assetTypeList = value;
            RaisePropertyChanged();
        }
    }

    private ObservableCollection<string> _assetTypeList;

    public string AssetType
    {
        get => _assetType;
        set
        {
            _assetType = value;
            RaisePropertyChanged();
        }
    }

    private string _assetType;
    private bool _isNew;

    #endregion

    #region Actions

    public List<string> ValidateViewModel()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(AssetName))
            errors.Add("Asset name is empty. Please provide asset name.");

        if (AssetAmount < 0m)
            errors.Add("Asset amount is negative. Please enter zero or positive number");

        if (AssetDate > DateTime.Today)
            errors.Add("Asset date is in the future. Please enter the date to be today or earlier");

        return errors;
    }

    public List<string> SaveViewModel()
    {
        var errors = ValidateViewModel();
        if (errors.Count > 0)
            return errors;

        var newAsset = new Asset(AssetId, AssetName, AssetAmount, AssetType, AssetDate);

        var (succeeded, errorMessage, result) = IsNew
            ? Parent.AssetService.AddAsset(newAsset)
            : Parent.AssetService.UpdateAsset(newAsset);

        if (!succeeded)
            errors.Add(errorMessage);

        return errors;
    }

    #endregion
}
