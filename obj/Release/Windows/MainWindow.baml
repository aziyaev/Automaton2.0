   M S B A M L   `   `   `  ���� D  @Labyrinth, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:Labyrinth	Labyrinth  O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089I2clr-namespace:System.Collections;assembly=mscorlibSystem.Collections     Labyrinth.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     )localclr-namespace:Labyrinth     Bcol2clr-namespace:System.Collections;assembly=mscorlib     -     � Title$     Лабиринт�� � Icon$ /Resources/MainIcon.png�����#dcdcdc$	��450��$	��800��$	��800��$	��450�� � 	Resources    WindowColor  ContentAreaColorLight  ContentAreaColorDark  DisabledControlLightColor  DisabledControlDarkColor  DisabledForegroundColor  SelectedBackgroundColor  SelectedUnfocusedColor  ControlLightColor 	 ControlMediumColor 
 ControlDarkColor  ControlMouseOverColor  ControlPressedColor  
GlyphColor  GlyphMouseOver  BorderLightColor  BorderMediumColor  BorderDarkColor  PressedBorderLightColor  PressedBorderDarkColor  DisabledBorderLightColor  DisabledBorderDarkColor  DefaultBorderBrushDarkColor  HeaderTopColor " DatagridCurrentCellBorderColor  SliderTrackDarkColor  NavButtonFrameColor  MenuPopupBrush �� EndPoint �� 
StartPoint � Color � Offset $  ProgressBarIndicatorAnimatedFill �� 
TargetType �� Property	 �� Value  RadioButtonFocusVisual
 �� 
TargetType �� 
Background �� Bullet A�Name �� Color'  !System.Windows.VisualStateManager   VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState   
Storyboard �� 
TargetName �� TargetProperty8  2System.Windows.Media.Animation.EasingColorKeyFrame �� KeyTime �� Value R� KeyTime  System.Windows.Visibility   Visible R� Value��%
  &	        &	      &	 &     &	 9     &	 L     &	 _     &	 r     &	 �     &	 �     &		 �     &	
 �     &	 �     &	 �     &	 �     &	     &	 @    &	 O    &	 ^    &	 m    &	 �    &	 �    &	 �    &	 �    &	 �    &	 �    &	 �    &	 �    &	     &	 n    '	�     7  7  7  ��	#FFE8EDF9����	#FFC5CBF9����	#FF7381F9����	#FFE8EDF9����	#FFC5CBF9����	#FF888888����	#FFC5CBF9����	#FFDDDDDD����Transparent����Transparent����Transparent����	#FF3843C4����	#FF211AA9����Gray����/+sc#1, 0.004391443, 0.002428215, 0.242281124����	Black����	White����	White����	#FF888888����	#FF444444����	#FFAAAAAA����	#FF888888����	Black����	#FFC5CBF9����	Black����	#FFC5CBF9����	#FF3843C4���� $ 0.5,1&�$ 0.5,0&�.��� # �  $ 0Q�� # � 	 $	 0.5Q�� # �  $ 1Q��� $	 0,0&�$	 1,0&���� � $ 	#000000FF��$ 0Q�� $ 	#600000FF��$	 0.4Q�� $ 	#600000FF��$	 0.6Q�� $ 	#000000FF��$ 1Q��� " 	�.���  �@J�SnapsToDevicePixels	 . ��  �@�OverridesDefaultStyle	 . ��  �@�FocusVisualStyle#	 �  ��  � ��	 �� "
 	�.R��� 
 ����� � $��20��$��20��.+�A� $ Border��$��1������ $ 0.5,1&�$ 0.5,0&�.��� # �  $ 0Q�� # �  $ 1Q����� $	 0,0&�$	 0,1&���� � # �  � # � 	 $	 1.0Q�A� $ 	CheckMark��$��4q�$y�	Collapsed=����� # �     CommonStates��.   
Normal��  	MouseOver��. �� .���� $ Border��$] W(Shape.Fill).                     (GradientBrush.GradientStops)[1].(GradientStop.Color)�.g�  $ 0��8     Pressed��. �� .���� $ Border��$] W(Shape.Fill).                     (GradientBrush.GradientStops)[1].(GradientStop.Color)�.g�  $ 0��8    Disabled��. �� .���� $ Border��$] W(Shape.Fill).                     (GradientBrush.GradientStops)[1].(GradientStop.Color)�.g�  $ 0��8  �� $ Border��$_ Y(Shape.Stroke).                     (GradientBrush.GradientStops)[1].(GradientStop.Color)�.g�  $ 0��$ 	#40000000���� $ Border��$_ Y(Shape.Stroke).                     (GradientBrush.GradientStops)[0].(GradientStop.Color)�.g�  $ 0��$ 	#40000000��  CheckStates��.   Checked��. �� .��T� $ (UIElement.Visibility)�$ 	CheckMark��..�i� $ 0��# Z   	Unchecked��  Indeterminate��._��� $��6,0,0,0q�$��Center=�$
��Left=���. .��� .+�]� ��. .+�o� $��25��$	��Top=�.@�m� $��Файл��.@�m� -   $��
Новый��m� -   $��Открыть���� m� -   $ ��Переименовать���� m� -   $��Сохранить��m� -   $��Сохранить как���� m� -   $��Закрыть файл��m� -   $��
Выйти��m� $��Правка��.@�m� -	   $#��Закрасить белым��m� -
   $%��Закрасить чёрным���� m� -   $��Очистить��m� $��Вид��.@�m� -   $��Линии сетки���� m� -   $X��RВключить/Отключить отрисовку следа автомата���� m� -   $;��5Убрать путь текущего автомат��m� -   $7��1Убрать пути всех автоматов��m� $��Инструменты��.@�m� -   $��Карандаш��m� -   $��Ластик��m� -   $��Заливка���� m� -   $!��Правая граница��m� -   $��Левая граница��m� -   $#��Верхняя граница��m� -   $!��Нижняя граница���� m� -   $��
Линия��m� -   $��Функция��m� $��Масштаб��.@�m� -   $��Приблизить��m� -   $��Отдалить���� m� -   $��По умолчанию��m� -   $(��"Вернуться в начало��m� $��Автомат��.@�m� -   $)��#Установить автомат��m� -   $0��*Добавить новый автомат��m� -   $2��,Удалить текущий автомат��m� -    $J��DЗадать таблицу для текущего автомата���� m� -!   $,��&Старт всех автоматов��m� -"   $(��"Шаг всех автоматов��m� -#   $��
Старт��m� -$   $��Стоп��m� -%   $��Шаг��m� $0��*Поиск кратчайшего пути��.@�m� -&   $(��"Установить точку А��m� -'   $(��"Установить точку Б���� m� -(   $��Найти путь��m� $��Справка��.@�m� -)   $��О программе��c�  `� IsLocked . $��35�����	#FFE7E7E7$	��Top=�.@��� -*   ��Z� ��Z� .���� $��/Resources/NewFile.png�� J� OpacityMask
 �   ��� -+   ��Z� ��Z� .���� $��/Resources/Save.png��
 �   ��� �� �� -,    ��Name$ buttonFillWhite����Z� ��Z� .���� $��/Resources/FillWhite.png��
 �   ��� --   $ buttonFillDark����Z� ��Z� .���� $��/Resources/FillBlack.png��
 �   ��� -.   $ buttonClear����Z� ��Z� .���� $.��(/Labyrinth;component/Resources/Erase.png��
 �   ��� �� �� -/   $ buttonSetAutomatonStart����Z� ��Z� .���� $��/Resources/LetterA.png��
 �   ��� �� �� -0   $ ButtonStartAutomaton����Z� ��Z� .���� $��/Resources/ButtonPlay.png��
 �   ��� -1   $ ButtonStopAutomaton����Z� ��Z� .���� $��/Resources/ButtonStop.png��
 �   ��� �� �� -2   $ 	buttonPen����Z� ��Z� .���� $��/Resources/Pencil.png��
 �   ��� -3   $ buttonPencil����Z� ��Z� .���� $,��&/Labyrinth;component/Resources/Pen.png��
 �   ��� -4   $ buttonEraser����Z� ��Z� .���� $��/Resources/Eraser.png��
 �   ��� -5   $ 
buttonFill����Z� ��Z� .���� $��/Resources/Fill.png��
 �   ��� �� �� -6   $ buttonFillUp����Z� ��Z� .���� $��/Resources/Top.png��
 �   ��� -7   $ buttonFillDown����Z� ��Z� .���� $��/Resources/Bottom.png��
 �   ��� -8   $ buttonFillLeft����Z� ��Z� .���� $��/Resources/Left.png��
 �   ��� -9   $ buttonFillRight����Z� ��Z� .���� $��/Resources/Right.png��
 �   ��� �� �� -:   $ buttonDrawLine����Z� ��Z� .���� -;   $��/Resources/Line.png��
 �   ��� -<   $ buttonSetFunction����Z� ��Z� .���� $��/Resources/Function.png��
 �   ��� �� �� -=   $��ButtonIncrease����Z� ��Z� .���� $��/Resources/ZoomP.png��
 �   ��� ->   $��ButtonReduce����Z� ��Z� .���� $��/Resources/ZoomN.png��
 �   ��� �� �� -?   ��Z� ��Z� .���� $��/Resources/FindPath.png��
 �   ��  � ColumnDefinitions �� $
��177* ��� $
��407* ��� $	��200 �.+��� $��0��$��2��.@��� .���� -@    ��Name$ 	status_tb���� -A    ��Name$ tab_control�����#dcdcdc$��0��$��2��.@��� $��Лабиринт 1��.��� -B    �Name$ content��  � ShowGridLines  .  ! � RowDefinitions! �� $��0* ��� $	��86* ��� $	��17* � �� $��0* ��� $
��755* ��� $	��31* �.+�� -C   $ canvas_elements��$��3��$��2��$��0,1,0,0q�$��1�����	#FF72728B �� $	��61* ��� $
��725* �.+��� -D   " ��Name$" canvas_container�����	#FF72728B$��2��$��0q�# J� RenderTransformOrigin$# 0.5,0.5&�$��Center=�$��Center=�.+��� -E   $" canvas��$��Center=�$��Center=����	#FF9999B8$# 0.5,0.5&�$	��730��$	��288��
$ �� Top$$ 10��% J� SnapsToDevicePixels% . �� ���	#FFF0F0F0$��0,-43,-12,0q�$# 1,0.5&�$��Right=�$��31��$��1���� ���	#FFF0F0F0$
��Left=�$��42��$# 
2.53,0.491&�$��-15,-43,0,0q��� ���	#FFF0F0F0$��	0,-43,0,0q�$	��Top=�$# 0.5,0&�$��43��$��3���� $# 0.5,0.5&����	#FFAAAAAA$��2��$��2��$��
27,16,28,0q��� -F   & ��Name$& scrollV��$��2��$��1��' � Maximum$	' 100Q�$��	0,17,12,0q�$��17��$��Right=�
����������	#FF636363$# 1,0.5&��� -G   $& scrollH��$��1��( �� Orientation$( 
Horizontal=�$	' 100Q�$��2��$��	27,0,12,0q�$��1��$# 0.5,0.5&�$��17��$	��Top=�
����������	#FF636363z�V� .~��� �� �� ) �� Angle$) 0.069Q�T� � $��1��$��2��$��2��$��
-8,0,-21,0q� �� $��2* ��� $��* ��� $��2* �.+��� ���	#FFF0F0F0$��	0,0,0,-27q�$# 0.5,1&�$��3�����	#FFAAAAAA$��2��� $��1��$��0��$��
27,10,27,0q�! �� $��* ��� $��* � �� $��* ��� $��2* ��� $��* �.+��� -H   $ buttonUp��$��1��$��0�����	#FFF6FFF4���	#FF8B8B8B.���� $��/Resources/Up.png��$��35��
 �   ��� -I   $ 
buttonDown��$��1��$��1�����	#FFF6FFF4���	#FF8B8B8B.���� $��/Resources/Down.png��$��35��
 �   ��� -J   $ 
buttonLeft��$��0��$��0��$��2�����	#FFF6FFF4���	#FF8B8B8B.���� $��/Resources/LeftArrow.png��$��35��
 �   ��� -K   $ buttonRight��$��2��$��0��$��2�����	#FFF6FFF4���	#FF8B8B8B.���� $��/Resources/RightArrow.png��$��35��
 �   ��� -L   $��+��� $��2��.+�]� .+�c�  . $��35�����	#FFE7E7E7$	��Top=�.@��� -M   $ AddAutomaton����Z� ��Z� .���� $��/Resources/ButtonPlus.png��
 �   ��� -N   $ RemoveAutomaton����Z� ��Z� .���� $ ��/Resources/ButtonMinus.png��
 �   ��� -O   $
 Step����Z� ��Z� .���� $��/Resources/ButtonStep.png��
 �   ��� �� �� -P   $ 	ShowTable����Z� ��Z� .���� $��/Resources/Table.png��
 �   ��� �� �� -Q   $ SetSpeed����Z� ��Z� .���� $ ��/Resources/ButtonSpeed.png��
 �   ��� �� �� $��0��.���� -R   * ��Name$* AutomatonStackPanel��� -S   $ InputBox��$y�	Collapsed=�.+�� 
���   �+ J� Opacity$	+ 0.5Q��� $	��250��
�������
���   �$��1q�, �� CornerRadius$, 10,10,10,10��$��Center=�$��Center=�._��� .+��� $��5q�$6��0Введите скорость Автомата��$��Center=�$
��Bold"�$��Cambria)��� -T   - ��Name$- InputTextBox��$	��150��$��Center=�$��Center=��� . �� Orientation$. 
Horizontal=�$��Center=�.+��� -U   $ 	YesButton��$��2,5,2,5q�$��70��$��Установить����Z� �� -V   $ NoButton��$��2,5,2,5q�$��70��$��Отмена����Z� 