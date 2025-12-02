using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace VPet.Plugin.LuckyGame.Controls
{
    public partial class LotteryNumberRoller : UserControl
    {
        #region 依赖属性
        public static readonly DependencyProperty MainNumbersProperty =
            DependencyProperty.Register("MainNumbers", typeof(List<int>), typeof(LotteryNumberRoller),
                new PropertyMetadata(new List<int> { 0, 0, 0, 0, 0, 0 }, OnMainNumbersChanged));

        public static readonly DependencyProperty SpecialNumbersProperty =
            DependencyProperty.Register("SpecialNumbers", typeof(List<int>), typeof(LotteryNumberRoller),
                new PropertyMetadata(new List<int> { 0, 0 }, OnSpecialNumbersChanged));

        public static readonly DependencyProperty MainMinValueProperty =
            DependencyProperty.Register("MainMinValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(0));

        public static readonly DependencyProperty MainMaxValueProperty =
            DependencyProperty.Register("MainMaxValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(30));

        public static readonly DependencyProperty SpecialMinValueProperty =
            DependencyProperty.Register("SpecialMinValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(0));

        public static readonly DependencyProperty SpecialMaxValueProperty =
            DependencyProperty.Register("SpecialMaxValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(9));

        public static readonly DependencyProperty IsRollingProperty =
            DependencyProperty.Register("IsRolling", typeof(bool), typeof(LotteryNumberRoller),
                new PropertyMetadata(false));
        #endregion

        #region 属性
        public List<int> MainNumbers
        {
            get => (List<int>)GetValue(MainNumbersProperty);
            set => SetValue(MainNumbersProperty, value);
        }

        public List<int> SpecialNumbers
        {
            get => (List<int>)GetValue(SpecialNumbersProperty);
            set => SetValue(SpecialNumbersProperty, value);
        }

        public int MainMinValue
        {
            get => (int)GetValue(MainMinValueProperty);
            set => SetValue(MainMinValueProperty, value);
        }

        public int MainMaxValue
        {
            get => (int)GetValue(MainMaxValueProperty);
            set => SetValue(MainMaxValueProperty, value);
        }

        public int SpecialMinValue
        {
            get => (int)GetValue(SpecialMinValueProperty);
            set => SetValue(SpecialMinValueProperty, value);
        }

        public int SpecialMaxValue
        {
            get => (int)GetValue(SpecialMaxValueProperty);
            set => SetValue(SpecialMaxValueProperty, value);
        }

        public bool IsRolling
        {
            get => (bool)GetValue(IsRollingProperty);
            set => SetValue(IsRollingProperty, value);
        }
        #endregion

        private readonly Random _random;
        private List<TextBlock> _mainNumberTexts;
        private List<TextBlock> _specialNumberTexts;
        private bool _isInitialized = false;

        // 动画控制变量
        private DispatcherTimer _animationTimer;
        private DispatcherTimer _stopSequenceTimer;
        private int _currentFrame = 0;
        private int TotalFrames = 120; // 总帧数（4秒，30fps）
        private int _currentStopIndex = 0; // 当前要停止的数字索引
        private const double StopIntervalSeconds = 1.5; // 停止间隔（秒）
        private const int StopIntervalFrames = 15; // 停止间隔（帧数，0.5秒 * 30fps）

        // 每个数字的动画状态
        private class NumberAnimationState
        {
            public int CurrentDisplayValue { get; set; }
            public int FinalValue { get; set; }
            public int MinValue { get; set; }
            public int MaxValue { get; set; }
            public bool IsStopped { get; set; }
            public int StopFrame { get; set; } = -1; // 停止的帧数，-1表示未停止
        }

        private List<NumberAnimationState> _mainNumberStates;
        private List<NumberAnimationState> _specialNumberStates;

        public LotteryNumberRoller()
        {
            InitializeComponent();
            _random = new Random();
            InitializeVisualElements();
            InitializeAnimationTimer();
            InitializeStopSequenceTimer();
        }

        private void InitializeAnimationTimer()
        {
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 约30fps
            _animationTimer.Tick += OnAnimationTimerTick;
        }

        private void InitializeStopSequenceTimer()
        {
            _stopSequenceTimer = new DispatcherTimer();
            _stopSequenceTimer.Interval = TimeSpan.FromSeconds(StopIntervalSeconds);
            _stopSequenceTimer.Tick += OnStopSequenceTimerTick;
        }

        private void InitializeVisualElements()
        {
            _mainNumberTexts = new List<TextBlock>
            {
                MainNumber1Text, MainNumber2Text, MainNumber3Text,
                MainNumber4Text, MainNumber5Text, MainNumber6Text
            };

            _specialNumberTexts = new List<TextBlock>
            {
                SpecialNumber1Text, SpecialNumber2Text
            };

            // 初始化显示
            UpdateNumbersDisplay();
            _isInitialized = true;
        }

        private static void OnMainNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LotteryNumberRoller)d;
            control.UpdateNumbersDisplay();
        }

        private static void OnSpecialNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LotteryNumberRoller)d;
            control.UpdateNumbersDisplay();
        }

        private void UpdateNumbersDisplay()
        {
            if (!_isInitialized) return;

            // 安全更新主号码显示
            for (int i = 0; i < 6; i++)
            {
                if (i < _mainNumberTexts.Count && i < MainNumbers.Count)
                {
                    _mainNumberTexts[i].Text = MainNumbers[i].ToString();
                }
                else if (i < _mainNumberTexts.Count)
                {
                    _mainNumberTexts[i].Text = "0";
                }
            }

            // 安全更新副号码显示
            for (int i = 0; i < 2; i++)
            {
                if (i < _specialNumberTexts.Count && i < SpecialNumbers.Count)
                {
                    _specialNumberTexts[i].Text = SpecialNumbers[i].ToString();
                }
                else if (i < _specialNumberTexts.Count)
                {
                    _specialNumberTexts[i].Text = "0";
                }
            }
        }

        /// <summary>
        /// 开始滚动动画
        /// </summary>
        public void StartRollingAnimation(double durationSeconds = 4.0)
        {
            if (IsRolling || !_isInitialized)
                return;

            TotalFrames = (int)(durationSeconds * 30); // 根据持续时间调整总帧数
            IsRolling = true;
            _currentFrame = 0;
            _currentStopIndex = 0;

            // 验证号码数据
            ValidateNumbers();

            // 初始化动画状态
            InitializeAnimationStates();

            // 重置所有数字显示为随机值
            ResetNumberDisplays();

            // 恢复文本颜色
            ResetTextColors();

            // 启动动画定时器
            _animationTimer.Start();

            // 延迟一段时间后开始顺序停止（让数字先滚动一会儿）
            var startStopTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(durationSeconds) };
            startStopTimer.Tick += (s, e) =>
            {
                startStopTimer.Stop();
                _stopSequenceTimer.Start();
                StartNextNumberStop(); // 开始第一个数字的停止
            };
            startStopTimer.Start();

            // 触发开始事件
            RollingStarted?.Invoke(this, EventArgs.Empty);
        }

        private void ValidateNumbers()
        {
            // 确保主号码有6个
            while (MainNumbers.Count < 6)
            {
                MainNumbers.Add(0);
            }

            // 确保副号码有2个
            while (SpecialNumbers.Count < 2)
            {
                SpecialNumbers.Add(0);
            }
        }

        private void InitializeAnimationStates()
        {
            _mainNumberStates = new List<NumberAnimationState>();
            _specialNumberStates = new List<NumberAnimationState>();

            // 初始化主号码动画状态
            for (int i = 0; i < 6; i++)
            {
                if (i < MainNumbers.Count)
                {
                    _mainNumberStates.Add(new NumberAnimationState
                    {
                        CurrentDisplayValue = _random.Next(MainMinValue, MainMaxValue + 1),
                        FinalValue = MainNumbers[i],
                        MinValue = MainMinValue,
                        MaxValue = MainMaxValue,
                        IsStopped = false,
                        StopFrame = -1
                    });
                }
            }

            // 初始化副号码动画状态
            for (int i = 0; i < 2; i++)
            {
                if (i < SpecialNumbers.Count)
                {
                    _specialNumberStates.Add(new NumberAnimationState
                    {
                        CurrentDisplayValue = _random.Next(SpecialMinValue, SpecialMaxValue + 1),
                        FinalValue = SpecialNumbers[i],
                        MinValue = SpecialMinValue,
                        MaxValue = SpecialMaxValue,
                        IsStopped = false,
                        StopFrame = -1
                    });
                }
            }
        }

        private void ResetNumberDisplays()
        {
            // 重置主号码显示为随机值
            for (int i = 0; i < 6 && i < _mainNumberTexts.Count && i < _mainNumberStates.Count; i++)
            {
                _mainNumberTexts[i].Text = _mainNumberStates[i].CurrentDisplayValue.ToString();
            }

            // 重置副号码显示为随机值
            for (int i = 0; i < 2 && i < _specialNumberTexts.Count && i < _specialNumberStates.Count; i++)
            {
                _specialNumberTexts[i].Text = _specialNumberStates[i].CurrentDisplayValue.ToString();
            }
        }

        private void ResetTextColors()
        {
            // 重置主号码颜色
            foreach (var textBlock in _mainNumberTexts)
            {
                textBlock.Foreground = (Brush)FindResource("SuccessBrush");
            }

            // 重置副号码颜色
            foreach (var textBlock in _specialNumberTexts)
            {
                textBlock.Foreground = (Brush)FindResource("DangerBrush");
            }
        }

        private void OnAnimationTimerTick(object sender, EventArgs e)
        {
            _currentFrame++;

            // 计算动画进度 (0.0 - 1.0)
            double progress = (double)_currentFrame / TotalFrames;

            // 更新数字显示（只更新未停止的数字）
            UpdateNumberAnimations(progress);

            // 检查动画是否完成（所有数字都已停止）
            if (AreAllNumbersStopped())
            {
                CompleteAnimation();
            }
        }

        private void OnStopSequenceTimerTick(object sender, EventArgs e)
        {
            // 停止当前数字并开始下一个数字的停止
            StopCurrentNumber();
            StartNextNumberStop();
        }

        private void StartNextNumberStop()
        {
            // 如果所有数字都已停止，停止定时器
            if (_currentStopIndex >= 8)
            {
                _stopSequenceTimer.Stop();
                return;
            }

            // 为下一个数字准备停止（减速效果）
            PrepareNumberForStop(_currentStopIndex);
        }

        private void PrepareNumberForStop(int numberIndex)
        {
            // 这里可以添加减速效果，比如降低滚动速度
            // 当前实现中，数字会在下一次动画更新时自然停止
        }

        private void StopCurrentNumber()
        {
            if (_currentStopIndex >= 8) return;

            NumberAnimationState state = GetNumberState(_currentStopIndex);
            TextBlock textBlock = GetNumberTextBlock(_currentStopIndex);

            if (state != null && textBlock != null && !state.IsStopped)
            {
                // 停止这个数字
                state.CurrentDisplayValue = state.FinalValue;
                state.IsStopped = true;
                state.StopFrame = _currentFrame;
                textBlock.Text = state.FinalValue.ToString();

                // 应用高亮效果
                textBlock.Foreground = Brushes.Gold;

                // 触发数字停止事件
                NumberStopped?.Invoke(this, new NumberStoppedEventArgs
                {
                    NumberIndex = _currentStopIndex,
                    FinalValue = state.FinalValue,
                    IsMainNumber = _currentStopIndex < 6
                });

                // 移动到下一个数字
                _currentStopIndex++;
            }
            else
            {
                // 如果当前数字已经停止或无效，直接移动到下一个
                _currentStopIndex++;
            }

            // 如果所有数字都已停止，停止定时器
            if (_currentStopIndex >= 8)
            {
                _stopSequenceTimer.Stop();
            }
        }

        private void UpdateNumberAnimations(double progress)
        {
            // 计算当前滚动速度（只对未停止的数字有效）
            int speed = CalculateCurrentSpeed(progress);

            // 更新主号码动画（只更新未停止的）
            for (int i = 0; i < _mainNumberStates.Count; i++)
            {
                if (!_mainNumberStates[i].IsStopped)
                {
                    UpdateSingleNumberAnimation(_mainNumberStates[i], _mainNumberTexts[i], speed, progress, i);
                }
            }

            // 更新副号码动画（只更新未停止的）
            for (int i = 0; i < _specialNumberStates.Count; i++)
            {
                if (!_specialNumberStates[i].IsStopped)
                {
                    UpdateSingleNumberAnimation(_specialNumberStates[i], _specialNumberTexts[i], speed, progress, i + 6);
                }
            }
        }

        private int CalculateCurrentSpeed(double progress)
        {
            // 基础速度
            int baseSpeed = 5;

            // 根据进度调整速度
            if (progress < 0.3)
            {
                // 开始阶段：快速滚动
                return baseSpeed + 3;
            }
            else if (progress < 0.7)
            {
                // 中间阶段：中等速度
                return baseSpeed;
            }
            else
            {
                // 结束阶段：慢速（为停止做准备）
                return Math.Max(1, baseSpeed - 2);
            }
        }

        private void UpdateSingleNumberAnimation(NumberAnimationState state, TextBlock textBlock, int speed, double progress, int numberIndex)
        {
            if (state.IsStopped) return;

            // 继续滚动
            int newValue = state.CurrentDisplayValue + speed;

            // 处理数值边界（循环）
            if (newValue > state.MaxValue)
            {
                newValue = state.MinValue + (newValue - state.MaxValue - 1);
            }
            else if (newValue < state.MinValue)
            {
                newValue = state.MaxValue - (state.MinValue - newValue - 1);
            }

            state.CurrentDisplayValue = newValue;
            textBlock.Text = newValue.ToString();

            // 如果这个数字即将停止，添加视觉反馈
            if (numberIndex == _currentStopIndex && progress > 0.3)
            {
                textBlock.Foreground = Brushes.Orange;
            }
        }

        private NumberAnimationState GetNumberState(int numberIndex)
        {
            if (numberIndex < 6)
            {
                return numberIndex < _mainNumberStates.Count ? _mainNumberStates[numberIndex] : null;
            }
            else
            {
                int specialIndex = numberIndex - 6;
                return specialIndex < _specialNumberStates.Count ? _specialNumberStates[specialIndex] : null;
            }
        }

        private TextBlock GetNumberTextBlock(int numberIndex)
        {
            if (numberIndex < 6)
            {
                return numberIndex < _mainNumberTexts.Count ? _mainNumberTexts[numberIndex] : null;
            }
            else
            {
                int specialIndex = numberIndex - 6;
                return specialIndex < _specialNumberTexts.Count ? _specialNumberTexts[specialIndex] : null;
            }
        }

        private bool AreAllNumbersStopped()
        {
            // 检查主号码
            foreach (var state in _mainNumberStates)
            {
                if (!state.IsStopped) return false;
            }

            // 检查副号码
            foreach (var state in _specialNumberStates)
            {
                if (!state.IsStopped) return false;
            }

            return true;
        }

        private void CompleteAnimation()
        {
            _animationTimer.Stop();
            _stopSequenceTimer.Stop();
            IsRolling = false;

            // 确保所有数字都显示最终值
            for (int i = 0; i < _mainNumberStates.Count; i++)
            {
                _mainNumberStates[i].CurrentDisplayValue = _mainNumberStates[i].FinalValue;
                _mainNumberStates[i].IsStopped = true;
                if (i < _mainNumberTexts.Count)
                {
                    _mainNumberTexts[i].Text = _mainNumberStates[i].FinalValue.ToString();
                    _mainNumberTexts[i].Foreground = Brushes.Gold;
                }
            }

            for (int i = 0; i < _specialNumberStates.Count; i++)
            {
                _specialNumberStates[i].CurrentDisplayValue = _specialNumberStates[i].FinalValue;
                _specialNumberStates[i].IsStopped = true;
                if (i < _specialNumberTexts.Count)
                {
                    _specialNumberTexts[i].Text = _specialNumberStates[i].FinalValue.ToString();
                    _specialNumberTexts[i].Foreground = Brushes.Gold;
                }
            }
            Task.Run(async () =>
            {
                await Task.Delay(Convert.ToInt32(500 * StopIntervalSeconds));
                _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    RollingCompleted?.Invoke(this, EventArgs.Empty);
                }));
            });
            
        }

        /// <summary>
        /// 停止滚动并立即显示最终结果
        /// </summary>
        public void StopRolling()
        {
            if (!IsRolling) return;

            _animationTimer.Stop();
            _stopSequenceTimer.Stop();

            // 立即停止所有数字
            for (int i = 0; i < 8; i++)
            {
                NumberAnimationState state = GetNumberState(i);
                TextBlock textBlock = GetNumberTextBlock(i);

                if (state != null && textBlock != null)
                {
                    state.CurrentDisplayValue = state.FinalValue;
                    state.IsStopped = true;
                    textBlock.Text = state.FinalValue.ToString();
                    textBlock.Foreground = Brushes.Gold;
                }
            }

            CompleteAnimation();
        }

        /// <summary>
        /// 设置最终数字
        /// </summary>
        public void SetFinalNumbers(byte[] mainNumbers, byte[] specialNumbers)
        {
            if (mainNumbers.Length == 6)
            {
                var newMainNumbers = new List<int>();
                foreach (var num in mainNumbers)
                    newMainNumbers.Add(Convert.ToInt32(num));
                MainNumbers = newMainNumbers;
            }

            if (specialNumbers.Length == 2)
            {
                var newSpecialNumbers = new List<int>();
                foreach (var num in specialNumbers)
                    newSpecialNumbers.Add(Convert.ToInt32(num));
                SpecialNumbers = newSpecialNumbers;
            }

            UpdateNumbersDisplay();
        }

        /// <summary>
        /// 随机生成号码
        /// </summary>
        public void GenerateRandomNumbers()
        {
            var mainNumbers = new List<int>();
            var specialNumbers = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                mainNumbers.Add(_random.Next(MainMinValue, MainMaxValue + 1));
            }

            for (int i = 0; i < 2; i++)
            {
                specialNumbers.Add(_random.Next(SpecialMinValue, SpecialMaxValue + 1));
            }

            MainNumbers = mainNumbers;
            SpecialNumbers = specialNumbers;
            UpdateNumbersDisplay();
        }

        /// <summary>
        /// 清空所有号码
        /// </summary>
        public void ClearNumbers()
        {
            MainNumbers = new List<int> { 0, 0, 0, 0, 0, 0 };
            SpecialNumbers = new List<int> { 0, 0 };
            UpdateNumbersDisplay();
        }

        /// <summary>
        /// 滚动动画开始事件
        /// </summary>
        public event EventHandler RollingStarted;

        /// <summary>
        /// 滚动动画完成事件
        /// </summary>
        public event EventHandler RollingCompleted;

        /// <summary>
        /// 单个数字停止事件
        /// </summary>
        public event EventHandler<NumberStoppedEventArgs> NumberStopped;
    }

    /// <summary>
    /// 数字停止事件参数
    /// </summary>
    public class NumberStoppedEventArgs : EventArgs
    {
        public int NumberIndex { get; set; }     // 数字索引（0-5主号码，6-7副号码）
        public int FinalValue { get; set; }       // 最终显示的值
        public bool IsMainNumber { get; set; }    // 是否为主号码
    }
}