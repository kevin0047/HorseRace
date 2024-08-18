using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HorseRace
{
    public partial class MainWindow : Window
    {
        // 말의 너비와 높이 상수
        private const int HorseWidth = 50;
        private const int HorseHeight = 20;

        // 랜덤 숫자를 생성하기 위한 Random 객체
        private Random _random = new Random();

        // 말의 사각형과 텍스트 블록을 저장할 리스트
        private List<Rectangle> _horses = new List<Rectangle>(); //Rectangle wpf에서 ui 사각형 그리는 도구
        private List<TextBlock> _horseNumbers = new List<TextBlock>();  

        // 각 말의 위치와 결과를 저장할 리스트
        private List<int> _horsePositions = new List<int>();
        private List<int> _results = new List<int>();
        private int _finishedCount = 0; //말의 도착순위를 세는 카운트
        public MainWindow()
        {
            InitializeComponent();
        }

        // 경기 시작 버튼 클릭 시 호출되는 메서드
        private void StartRace_Click(object sender, RoutedEventArgs e)
        {
            int horseCount;

            // 이전 결과를 지우기 위해 ResultTextBlock(경기결과)의 텍스트를 빈 문자열로 설정
            ResultTextBlock.Text = "";

            // 말의 마릿수 입력값을 확인하고 유효성을 검사
            if (!int.TryParse(HorseCountTextBox.Text, out horseCount) || horseCount < 2 || horseCount > 8)
            {
                // 유효하지 않은 경우 메시지 박스 표시
                MessageBox.Show("말의 마릿수를 입력하세요 2 ~ 8");
                return;
            }

            // 유효한 경우 StartRace 메서드를 호출하여 경기를 시작
            StartRace(horseCount);
        }

        // 경기를 시작하고 각 말을 화면에 배치하는 메서드
        private void StartRace(int horseCount)
        {
            // 이전의 말 및 텍스트 블록 제거
            RaceCanvas.Children.Clear();
            _horses.Clear();
            _horseNumbers.Clear();
            _horsePositions.Clear();
            _finishedCount = 0;//도착등수 카운트 초기화
            _results.Clear();
            
            // 말의 수만큼 반복하여 말과 번호를 생성
            for (int i = 0; i < horseCount; i++)
            {
                // 말의 사각형을 생성하고 설정
                var horse = new Rectangle // var은 타입을 자동으로 추론함. 지역변수에만 사용
                {
                    Width = HorseWidth,
                    Height = HorseHeight,
                    Fill = new SolidColorBrush(GetRandomColor()) // 랜덤 색상 설정
                };

                // 말 번호를 표시할 TextBlock 생성 및 설정
                var horseNumber = new TextBlock
                {
                    Text = (i + 1).ToString(), // 말 번호 설정
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                // 말과 번호의 위치를 설정
                Canvas.SetTop(horse, i * (HorseHeight + 10));
                Canvas.SetLeft(horse, 0);
                RaceCanvas.Children.Add(horse);
                RaceCanvas.Children.Add(horseNumber);

                // TextBlock의 초기 위치를 설정
                Canvas.SetTop(horseNumber, i * (HorseHeight + 10));
                // TextBlock의 위치를 말의 중앙으로 조정
                Canvas.SetLeft(horseNumber, (HorseWidth - horseNumber.ActualWidth) / 2); // 수평 중앙 정렬

                // 생성한 말과 텍스트 블록을 리스트에 추가
                _horses.Add(horse);
                _horseNumbers.Add(horseNumber);
                _horsePositions.Add(0); // 초기 위치는 0
                _results.Add(0); // 결과 초기화
            }

            // 각 말에 대해 독립적인 태스크를 생성
            var tasks = new List<Task>();
            for (int i = 0; i < horseCount; i++)
            {
                int index = i;

                // 각 말이 이동하는 동작을 태스크로 실행
                tasks.Add(Task.Run(() => MoveHorse(index)));//람다식
            }

            // 모든 태스크가 완료된 후(whenall) 결과를 표시
            Task.WhenAll(tasks).ContinueWith(t =>
            {
                // UI 스레드에서 결과 표시
                Dispatcher.Invoke(() =>
                {
                    ShowResults();
                });
            });
        }

        // 각 말이 경주를 하는 메서드
        private void MoveHorse(int index)
        {


            
            // 말이 결승선에 도달할 때까지 반복
            while (_horsePositions[index] < RaceCanvas.ActualWidth - HorseWidth)
            {
                int speed = _random.Next(10, 15); // 말의 속도 설정 (10 ~ 15의 랜덤 값)
                // UI 업데이트를 위해 Dispatcher.Invoke 사용
                Dispatcher.Invoke(() =>
                {
                    Canvas.SetLeft(_horses[index], _horsePositions[index]);
                    // TextBlock의 위치도 업데이트
                    Canvas.SetLeft(_horseNumbers[index], _horsePositions[index] + (HorseWidth - _horseNumbers[index].ActualWidth) / 2);
                });

                // 말의 위치를 업데이트하고 이동
                _horsePositions[index] += speed;

                // 이동 속도와 시뮬레이션 시간 설정
                Thread.Sleep(_random.Next(50, 150));
            }


            Dispatcher.Invoke(() =>
            {
                Canvas.SetLeft(_horses[index], RaceCanvas.ActualWidth - HorseWidth);
                Canvas.SetLeft(_horseNumbers[index], RaceCanvas.ActualWidth - HorseWidth + (HorseWidth - _horseNumbers[index].ActualWidth) / 2);
                _results[_finishedCount++] = index; // 도착한 순서대로 저장 _results리스트의 [말선착순카운트++] = 말번호
            });
        }

        // 경주 결과를 표시하는 메서드
        private void ShowResults()
        {
            
            var Results = new List<int>(_results);
            

            // 결과 텍스트를 초기화하고 결과를 추가
            ResultTextBlock.Text = "경기 결과:\n";
            for (int i = 0; i < Results.Count; i++)
            {
                ResultTextBlock.Text += $"{i + 1} 등: 말 {Results[i] + 1}번\n";
            }

            // 스크롤 뷰어를 가장 위로 이동 (결과가 많이 길어질 수 있으므로)
            ResultScrollViewer.ScrollToTop();
        }

        // 랜덤 색상을 생성하는 메서드
        private Color GetRandomColor()
        {
            return Color.FromRgb((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256));
        }

        // 텍스트 박스가 포커스를 얻었을 때 호출되는 메서드
        private void HorseCountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // 초기 텍스트가 표시된 상태일 때 클리어
            if (HorseCountTextBox.Text == "말의 마릿수를 입력하세요 2 ~ 8")
            {
                HorseCountTextBox.Text = "";
                HorseCountTextBox.Foreground = Brushes.Black;
            }
        }

        // 텍스트 박스가 포커스를 잃었을 때 호출되는 메서드
        private void HorseCountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // 텍스트 박스가 비어있으면 초기 텍스트로 설정
            if (string.IsNullOrWhiteSpace(HorseCountTextBox.Text))
            {
                HorseCountTextBox.Text = "말의 마릿수를 입력하세요 2 ~ 8";
                HorseCountTextBox.Foreground = Brushes.Gray;
            }
        }
    }
}
