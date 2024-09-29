using LayoutDesigner.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Input;
using LayoutDesigner.AdditionalClasses;

namespace LayoutDesigner.ViewModel
{
    public sealed class LayoutDesignerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static ObservableCollection<ReceiptProperty>? DynamicProperties { get; set; }

        private static string? _layoutContent;

        public string? LayoutContent
        {
            get => _layoutContent;
            set
            {
                _layoutContent = value;
                OnPropertyChanged();
            }
        }

        private FlowDocument _printPreviewText;

        public FlowDocument PrintPreviewText
        {
            get => _printPreviewText;
            set
            {
                _printPreviewText = value;
                OnPropertyChanged();
            }
        }

        public ICommand PrintButton { get; }

        public ICommand SaveButton { get; }

        public ICommand OpenButton { get; }

        public ICommand BoldButton { get; }

        public ICommand InsertProperty { get; }

        public LayoutDesignerViewModel()
        {
            _printPreviewText = new FlowDocument();
            DynamicProperties = new ObservableCollection<ReceiptProperty>();
            PrintButton = new CommandsHandler(_ => Print());
            SaveButton = new CommandsHandler(_ => Save(@"c:\LayoutDesigner"));
            OpenButton = new CommandsHandler(_ => Open());
            BoldButton = new CommandsHandler(_ => Bold());
            InsertProperty = new CommandsHandler(Insert);
        }

        public void LoadJsonProperties(string? jsonFilePath)
        {
            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                var jsonProperties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                DynamicProperties?.Clear();

                if (jsonProperties != null)
                    foreach (var property in jsonProperties)
                    {
                        object value = property.Value;

                        DynamicProperties?.Add(new ReceiptProperty
                        {
                            PropertyName = property.Key,
                            Value = value
                        });
                    }
            }
        }


        public void Insert(object parameter)
        {
            if (parameter is string propertyName)
            {
                if (Application.Current.MainWindow != null)
                {
                    var leftTextBox = Application.Current.MainWindow.FindName("LeftTextField") as TextBox;
                    if (leftTextBox != null)
                    {
                        int cursorPosition = leftTextBox.SelectionStart;
                        leftTextBox.Text = leftTextBox.Text.Insert(cursorPosition, $"{{{propertyName}}}");
                    }
                }
            }
        }

        private static void Bold()
        {
            if (Application.Current.MainWindow != null)
            {
                var leftTextBox = Application.Current.MainWindow.FindName("LeftTextField") as TextBox;
                if (!string.IsNullOrEmpty(leftTextBox?.SelectedText) && !leftTextBox.SelectedText.StartsWith($"<b>"))
                {
                    string selectedText = leftTextBox.SelectedText;
                    string newText = $"<b>{selectedText}</b>";
                    leftTextBox.Text = leftTextBox.Text.Replace(selectedText, newText);
                }
            }
        }

        public static void CalculateTotalAmount()
        {
            if (DynamicProperties != null)
            {
                var itemQuantityProperty = DynamicProperties.FirstOrDefault(p => p.PropertyName == "ItemQuantity");
                var itemPriceProperty = DynamicProperties.FirstOrDefault(p => p.PropertyName == "ItemPrice");

                if (itemQuantityProperty != null && itemPriceProperty != null)
                {
                    if (float.TryParse(itemQuantityProperty.Value.ToString(), out float quantity) &&
                        float.TryParse(itemPriceProperty.Value.ToString(), out float price))
                    {
                        if (quantity <= 0 || price <= 0)
                        {
                            MessageBox.Show("ItemQuantity and ItemPrice must be greater than zero.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var totalAmount = quantity * price;
                        var totalAmountProperty =
                            DynamicProperties.FirstOrDefault(p => p.PropertyName == "TotalAmount");
                        if (totalAmountProperty != null)
                        {
                            totalAmountProperty.Value = totalAmount.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
        }

        public void Print()
        {
            CalculateTotalAmount();
            var formattedText = "";
            if (LayoutContent != null)
            {
                string result = Regex.Replace(LayoutContent, @"\{(.*?)\}", match =>
                {
                    string key = match.Groups[1].Value;
                    string? value = null;
                    if (DynamicProperties != null)
                        foreach (var property in DynamicProperties)
                        {
                            if (property.PropertyName.Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                value = property.Value.ToString();
                                break;
                            }
                        }

                    return value ?? match.Value;
                });
                formattedText = result;
            }

            var paragraph = new Paragraph();
            var flowDoc = new FlowDocument();
            var boldRegex = new Regex(@"<b>(.*?)<\/b>");
            {
                var itemsToBold = boldRegex.Matches(formattedText);

                int lastIndex = 0;
                foreach (Match itemToBold in itemsToBold)
                {
                    if (itemToBold.Index > lastIndex)
                    {
                        string plainText = formattedText.Substring(lastIndex, itemToBold.Index - lastIndex);
                        paragraph.Inlines.Add(new Run(plainText));
                    }

                    string boldText = itemToBold.Groups[1].Value;
                    paragraph.Inlines.Add(new Bold(new Run(boldText)));
                    lastIndex = itemToBold.Index + itemToBold.Length;
                }

                if (lastIndex < formattedText.Length)
                {
                    paragraph.Inlines.Add(new Run(formattedText.Substring(lastIndex)));
                }
            }
            flowDoc.Blocks.Add(paragraph);
            PrintPreviewText = flowDoc;
        }


        public void Save(String directory)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "layoutContent.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, LayoutContent);
                    MessageBox.Show("The layout content was saved successfully.", "Success", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving the layout content: {ex.Message}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void Open()
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"c:\LayoutDesigner",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LayoutContent = File.ReadAllText(openFileDialog.FileName);
                    MessageBox.Show("the layout content file was loaded successfully.", "Success", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading layout content file: {ex.Message}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}