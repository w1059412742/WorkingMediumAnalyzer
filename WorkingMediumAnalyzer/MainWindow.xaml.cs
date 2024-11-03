using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkingMediumAnalyzer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Start();
            // 创建 Grid
            Grid dynamicGrid = new Grid();

            // 设置 Grid 行和列的数量
            int rows = 4; // 将5替换为你的行数
            int columns = 3; // 将3替换为你的列数

            for (int i = 0; i < rows; i++)
            {
                dynamicGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < columns; j++)
            {
                dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // 循环添加 Label 到每个单元格
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Label label = new Label
                    {
                        Content = i * columns + j + 1, //$"Row {i + 1}, Col {j + 1}",
                        Margin = new Thickness(10), // 设置边距为10像素
                        Background = Brushes.Red,
                        FontSize = 50,
                        FontWeight = FontWeights.Bold,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                    };
                    // 将 Label 添加到 Grid 并设置其位置
                    Grid.SetRow(label, i);
                    Grid.SetColumn(label, j);
                    dynamicGrid.Children.Add(label);

                }
            }
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {

                    if (i < rows - 1)
                    {
                        Label label2 = new Label
                        {
                            Content = 1856, //$"Row {i + 1}, Col {j + 1}",
                            Width = 100,
                            Height = 50,
                            Margin = new Thickness(0, 0, 0, -25), // 设置边距为10像素
                            Background = Brushes.Gray,
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalAlignment=VerticalAlignment.Bottom,
                            HorizontalAlignment= HorizontalAlignment.Center,
                        };
                        // 将 Label 添加到 Grid 并设置其位置
                        Grid.SetRow(label2, i);
                        Grid.SetColumn(label2, j);
                        dynamicGrid.Children.Add(label2);
                    }

                    if (j < columns - 1)
                    {
                        Label label3 = new Label
                        {
                            Content = 1856, //$"Row {i + 1}, Col {j + 1}",
                            Width = 100,
                            Height = 50,
                            Margin = new Thickness(0, 0, -50,0), // 设置边距为10像素
                            Background = Brushes.Gray,
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Right,
                        };
                        // 将 Label 添加到 Grid 并设置其位置
                        Grid.SetRow(label3, i);
                        Grid.SetColumn(label3, j);
                        dynamicGrid.Children.Add(label3);
                    }
                }
            }


            // 将 Grid 添加到窗口的主容器中
            this.Content = dynamicGrid;

        }

        public void Start()
        {
            int n = 12; // 电极数量
            int rows = 3; // 行数
            int cols = 4; // 列数

            // 生成所有电容对
            List<(int, int)> capacitors = GenerateAllCapacitors(n);

            // 标识每个电容是否为相邻电容或相对电容
            Dictionary<(int, int), (bool, bool)> capacitorAttributes = MarkAdjacentAndDiagonalCapacitors(rows, cols, capacitors);

            // 模拟电容值并进行初始化
            Dictionary<(int, int), double> capacitorValues = InitializeCapacitorValues(capacitors);

            // 标记不为0的电容值
            FilterCapacitorValues(capacitorAttributes, capacitorValues);

            // 寻找工质位置
            List<(int, int)> workSubstanceLocations = FindWorkSubstanceLocations(capacitorValues, rows, cols);

            // 处理多个工质并调整电容值
            AdjustCapacitanceForMultipleWorkSubstances(workSubstanceLocations, capacitorValues);

            // 导出与最大工质相关的电容集合
            List<(int, int)> relatedCapacitors = ExportRelatedCapacitors(workSubstanceLocations, capacitorValues);

            // 输出结果
            Console.WriteLine("与最大工质相关的电容: ");
            foreach (var cap in relatedCapacitors)
            {
                Console.WriteLine($"{cap.Item1}-{cap.Item2}");
            }
        }

        // 生成所有可能的电容对
        static List<(int, int)> GenerateAllCapacitors(int n)
        {
            List<(int, int)> capacitors = new List<(int, int)>();
            for (int i = 1; i <= n; i++)
            {
                for (int j = i + 1; j <= n; j++)
                {
                    capacitors.Add((i, j));
                }
            }
            return capacitors;
        }

        // 标记每个电容是否是相邻或相对电容
        static Dictionary<(int, int), (bool, bool)> MarkAdjacentAndDiagonalCapacitors(int rows, int cols, List<(int, int)> capacitors)
        {
            Dictionary<(int, int), (bool, bool)> attributes = new Dictionary<(int, int), (bool, bool)>();

            foreach (var cap in capacitors)
            {
                bool isAdjacent = IsAdjacent(cap.Item1, cap.Item2, rows, cols);
                bool isDiagonal = IsDiagonal(cap.Item1, cap.Item2, rows, cols);
                attributes[cap] = (isAdjacent, isDiagonal);
            }

            return attributes;
        }

        // 判断是否为相邻电容
        static bool IsAdjacent(int a, int b, int rows, int cols)
        {
            int rowA = (a - 1) / cols;
            int colA = (a - 1) % cols;
            int rowB = (b - 1) / cols;
            int colB = (b - 1) % cols;

            return (Math.Abs(rowA - rowB) == 1 && colA == colB) || (Math.Abs(colA - colB) == 1 && rowA == rowB);
        }

        // 判断是否为相对电容
        static bool IsDiagonal(int a, int b, int rows, int cols)
        {
            int rowA = (a - 1) / cols;
            int colA = (a - 1) % cols;
            int rowB = (b - 1) / cols;
            int colB = (b - 1) % cols;

            return Math.Abs(rowA - rowB) == 1 && Math.Abs(colA - colB) == 1;
        }

        // 初始化电容值
        static Dictionary<(int, int), double> InitializeCapacitorValues(List<(int, int)> capacitors)
        {
            Random rand = new Random();
            return capacitors.ToDictionary(cap => cap, cap => rand.NextDouble() * 10);
        }

        // 过滤电容值，标记为0的电容
        static void FilterCapacitorValues(Dictionary<(int, int), (bool, bool)> attributes, Dictionary<(int, int), double> values)
        {
            foreach (var cap in attributes)
            {
                if (!cap.Value.Item1 && !cap.Value.Item2)
                {
                    values[cap.Key] = 0;
                }
            }
        }

        // 查找工质位置
        /*static List<(int, int)> FindWorkSubstanceLocations(Dictionary<(int, int), double> values, int rows, int cols)
        {
            double threshold = values.Values.Max() * 0.8; // 假设的阈值来判断工质位置
            return values.Where(v => v.Value > threshold).Select(v => v.Key).ToList();
        }*/
        // 查找工质位置（优化版）
        static List<(int, int)> FindWorkSubstanceLocations(Dictionary<(int, int), double> values, int rows, int cols)
        {
            double maxCapacitance = values.Values.Max();
            double threshold = maxCapacitance * 0.8; // 假设的初始阈值
            List<(int, int)> potentialWorkSubstanceLocations = new List<(int, int)>();

            // 找出潜在的工质位置（电容值大于阈值）
            foreach (var cap in values)
            {
                if (cap.Value > threshold)
                {
                    potentialWorkSubstanceLocations.Add(cap.Key);
                }
            }

            // 优化分析：检查相邻电容以更精确确定工质位置
            List<(int, int)> refinedLocations = new List<(int, int)>();
            foreach (var location in potentialWorkSubstanceLocations)
            {
                int a = location.Item1;
                int b = location.Item2;

                // 检查与电容a和b相邻的电容
                var adjacentCapacitorsA = GetAdjacentCapacitors(a, rows, cols, values);
                var adjacentCapacitorsB = GetAdjacentCapacitors(b, rows, cols, values);

                // 计算相邻电容的平均值
                double avgAdjacentCapacitance = (adjacentCapacitorsA.Values.Sum() + adjacentCapacitorsB.Values.Sum()) /
                                                (adjacentCapacitorsA.Count + adjacentCapacitorsB.Count);

                // 如果平均值大于某个比例的最大电容值，则进一步考虑
                if (avgAdjacentCapacitance > maxCapacitance * 0.5)
                {
                    refinedLocations.Add(location);
                }
            }

            return refinedLocations;
        }
        // 获取与指定电极相邻的电容
        static Dictionary<(int, int), double> GetAdjacentCapacitors(int electrode, int rows, int cols, Dictionary<(int, int), double> values)
        {
            List<(int, int)> adjacentIndices = new List<(int, int)>();

            int row = (electrode - 1) / cols;
            int col = (electrode - 1) % cols;

            // 上下左右相邻
            if (row > 0) adjacentIndices.Add((electrode - cols, electrode)); // 上
            if (row < rows - 1) adjacentIndices.Add((electrode, electrode + cols)); // 下
            if (col > 0) adjacentIndices.Add((electrode - 1, electrode)); // 左
            if (col < cols - 1) adjacentIndices.Add((electrode, electrode + 1)); // 右

            // 提取实际存在的电容值
            Dictionary<(int, int), double> adjacentCapacitors = new Dictionary<(int, int), double>();
            foreach (var index in adjacentIndices)
            {
                if (values.ContainsKey(index))
                {
                    adjacentCapacitors[index] = values[index];
                }
            }

            return adjacentCapacitors;
        }

        // 调整多个工质的电容值
        static void AdjustCapacitanceForMultipleWorkSubstances(List<(int, int)> locations, Dictionary<(int, int), double> values)
        {
            double maxCapacitance = locations.Select(loc => values[loc]).Max();

            foreach (var loc in locations)
            {
                double current = values[loc];
                double scaleFactor = maxCapacitance / current;

                values[loc] = maxCapacitance;
                // 根据比例缩放其他相关电容
            }
        }

        // 导出与最大工质相关的电容
        static List<(int, int)> ExportRelatedCapacitors(List<(int, int)> locations, Dictionary<(int, int), double> values)
        {
            double maxCapacitance = locations.Select(loc => values[loc]).Max();
            return locations.Where(loc => values[loc] == maxCapacitance).ToList();
        }
    }
}
