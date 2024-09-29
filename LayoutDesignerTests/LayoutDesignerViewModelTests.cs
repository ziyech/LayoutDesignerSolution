using System.Collections.ObjectModel;
using LayoutDesigner.ViewModel;
using LayoutDesigner.Model;

namespace LayoutDesignerTests
{
    [TestFixture]
    public class LayoutDesignerViewModelTests
    {
        private LayoutDesignerViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _viewModel = new LayoutDesignerViewModel();
        }

        [Test]
        public void LoadJsonProperties_ValidJson_LoadsCorrectProperties()
        {
            var jsonContent = "{\"ItemQuantity\": \"5\", \"ItemPrice\": \"10\",\"TotalAmount\": \"50\"}";
            string jsonFilePath = @"c:\LayoutDesigner\propertiesForTests.json";
            File.WriteAllText(jsonFilePath, jsonContent);
            _viewModel.LoadJsonProperties(jsonFilePath);
            Assert.NotNull(LayoutDesignerViewModel.DynamicProperties);
            Assert.AreEqual(3, LayoutDesignerViewModel.DynamicProperties.Count);
            Assert.AreEqual("5", LayoutDesignerViewModel.DynamicProperties[0].Value.ToString());
            Assert.AreEqual("10", LayoutDesignerViewModel.DynamicProperties[1].Value.ToString());
            Assert.AreEqual("50", LayoutDesignerViewModel.DynamicProperties[2].Value.ToString());
            File.Delete(jsonFilePath);
        }

        [Test]
        public void CalculateTotalAmount_ValidProperties_CalculatesCorrectTotal()
        {
          
            LayoutDesignerViewModel.DynamicProperties = new ObservableCollection<ReceiptProperty>
            {
                new ReceiptProperty { PropertyName = "TotalAmount", Value = 50 },
                new ReceiptProperty { PropertyName = "ItemQuantity", Value = 5 },
                new ReceiptProperty { PropertyName = "ItemPrice", Value = 10 }
            };
            LayoutDesignerViewModel.CalculateTotalAmount();
            var totalAmount = LayoutDesignerViewModel.DynamicProperties[0].Value.ToString();
            Assert.AreEqual("50", totalAmount);
        }

        [Test]
        public void LayoutContentProperty_Setter_RaisesPropertyChanged()
        {
            bool propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LayoutDesignerViewModel.LayoutContent))
                    propertyChangedRaised = true;
            };
            _viewModel.LayoutContent = "New Content";
            Assert.IsTrue(propertyChangedRaised);
        }

        [Test]
        public void PrintButtonCommand_CanExecute_ReturnsTrue()
        {
            var canExecute = _viewModel.PrintButton.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Test]
        public void LoadJsonProperties_LoadsPropertiesCorrectly()
        {
            var jsonContent = "{\"ItemQuantity\": \"5\", \"ItemPrice\": \"10\", \"TotalAmount\": \"50\"}";
            var jsonFilePath = @"c:\LayoutDesigner\testProperties.json";
            File.WriteAllText(jsonFilePath, jsonContent);
            _viewModel.LoadJsonProperties(jsonFilePath);
            Assert.IsNotNull(LayoutDesignerViewModel.DynamicProperties);
            Assert.AreEqual(3, LayoutDesignerViewModel.DynamicProperties.Count);
            Assert.AreEqual("5", LayoutDesignerViewModel.DynamicProperties[0].Value.ToString());
        }

        [Test]
        public void CalculateTotalAmount_ValidValues_CalculatesCorrectly()
        {
            LayoutDesignerViewModel.DynamicProperties = new ObservableCollection<ReceiptProperty>
            {
                new() { PropertyName = "ItemQuantity", Value = "5" },
                new() { PropertyName = "ItemPrice", Value = "10" },
                new() { PropertyName = "TotalAmount", Value = "" }
            };
            LayoutDesignerViewModel.CalculateTotalAmount();
            var totalAmountProperty = LayoutDesignerViewModel.DynamicProperties.FirstOrDefault(p => p.PropertyName == "TotalAmount");
            Assert.AreEqual("50", totalAmountProperty?.Value.ToString());
        }

        [Test]
        public void SaveButtonCommand_CanExecute_ReturnsTrue()
        {
            var canExecute = _viewModel.SaveButton.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Test]
        public void OpenButtonCommand_CanExecute_ReturnsTrue()
        {
            var canExecute = _viewModel.OpenButton.CanExecute(null);
            Assert.IsTrue(canExecute);
        }
    }
}