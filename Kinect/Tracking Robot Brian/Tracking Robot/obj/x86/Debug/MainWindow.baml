   M S B A M L   `   `   `  ����  Tracking Robot/    )Microsoft.Kinect.TrackingRobot.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  
Robot Path��5   	   $	��768��6   $
��1366��6)    � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value  SquareCheckBox �� 
TargetType �� Orientation �Name	 ��Name
 ��Name �� TextAlignment �� Triggers P� Property P� Value �� 
TargetName��%1  &	        &	 #     &	 F     '�� i     &	 �     7  7  �� 5   
   $ 	#ff6e6e6e��62   �� 5   
   $ 	#ff52318f��64   �� 5	   
   $ 	#ff00BCF2��62   �� 5
   
   " ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      �� 5   
   " ��6   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� $ 
Horizontal=�6)   
������ .+�5   "   � $ SquareCheckBoxChecked��6'   .+�5   &   �� $	 CheckedNormal��6,   $��Images\CheckedNormal.png��6C   $
��None=�6e   $��Center=�6t   �� 5   &   $	 CheckedHover��6,   $��Images\CheckedHover.png��6B   $
��None=�6c   $��Center=�6r   $y�	Collapsed=�6�   5   #   � 5   "   $ SquareCheckBoxUnchecked��6'   $y�	Collapsed=�6H   .+�5   &   �� $	 UncheckedNormal��6,   $ ��Images\UncheckedNormal.png��6E   $
��None=�6i   $��Center=�6x   �� 5   &   $	 UncheckedHover��6,   $��Images\UncheckedHover.png��6D   $
��None=�6g   $��Center=�6v   $y�	Collapsed=�6�   5   #   �� 5   "   $
 SquareCheckBoxText��6,   #��z��6H   $
 Left=�6i   $��Center=�6   8��  $��15(�6�   $��9,0,0,0q�6�   5      5       5      P� 5       �@d�	IsChecked$ falseY�6;   . �5    "   �� $ SquareCheckBoxChecked��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5!   "   $ SquareCheckBoxUnchecked��6O    � y�$ Visible=�6?   6O   5"      P� 5#       �@J�IsMouseOver . . �5$   "   �� $ CheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5%   "   $ UncheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5&   "   $ CheckedHover��6O    � y�$ Visible=�6?   6O   �� 5'   "   $ UncheckedHover��6O    � y�$ Visible=�6?   6O   �� 5(   "   8  $ SquareCheckBoxText��6h    � ��5)      5+      5-      5.      5/      .��51      � -   5        $��
layoutGrid��51      $��	10 0 10 0q�6   $
��1273��60    � ColumnDefinitions 52   
   �� 53      $
��410* �6   �� 54      $	��501 �6   �� 55      $
��427* �6    � RowDefinitions 57   
   �� 58      $
��Auto �6   �� 59      $
��100* �6   �� 5:      $
��236* �6   �� 5;      $
��347* �6   �� 5<      $
��Auto �6   �� 5=      $
��Auto �6   .+�5?   
   ]� $��0,0,0,6q�6   $��3��6%   .+�5@      �� -   5        $��label1��5@   @   $��Current Hand Postion:��6   $��30��64   $	��148��6N   $
��Left=�6Z   $��Center=�6u   $��Center=�6�   $��Center=�6�   �� 5A      -   5        $��textBox1��5A   "   $��30��6   $��84��62   $
��Left=�6=   $��Center=�6X   $��Center=�6s   $��Center=�6�   �� 5B      -   5        $��label2��5B   B   $��Current Robot Position:��6   $��30��66   $	��154��6P   $
��Left=�6\   $��Center=�6w   $��Center=�6�   $��Center=�6�   �� 5C      -   5        $��textBox2��5C   "   $��30��6   $��84��62   $
��Left=�6=   $��Center=�6X   $��Center=�6s   $��Center=�6�   �� 5D      -   5        $��label3��5D   6   $��Robot Angle��6   $��30��6*   $��80��6D   $
��Left=�6O   $��Center=�6j   $��Center=�6�   $��Center=�6�   �� 5E      -   5        $��textBox3��5E   F   $��30��6   $��Center=�6"   $��Center=�6V   $��43��6x   $
��Left=�6�   $��Center=�6�   �� 5F      -	   5        $��label4��5F   5   $��
Node Angle��6   $��30��6)   $��80��6C   $
��Left=�6N   $��Center=�6i   $��Center=�6�   $��Center=�6�   �� 5G      -
   5        $��textBox4��5G   F   $��30��6   $��Center=�6"   $��Center=�6V   $��43��6x   $
��Left=�6�   $��Center=�6�   5H      (� 5I   
   $��1��6    (� Stretch$ Uniform=�6   $
��Left=�61   $��3��6L   $��3��6`   $	��905��6q   .��5J      �� -   5        $��Image��5J      $	��640��6!   $	��480��6-   5K      (� 5L   
   $��3��6   $��2��6   $ Uniform=�6/   $��right=�6A   $��2��6]   6p   �� 5M   
   -   5        $��	statusBar��5M   ?   $��4��6   $��Stretch=�6!   $��Bottom=�6P   
�������#��[  6~   $��3��6�   $��2��6�   $��10��6�   .@�5N      �� $��0 0 0 10q�6   61   5O      �� 5P   
   -   5        $��CImage��5P      $	��320��6   $	��240��6*   $��2��67   $��
17,0,83,96q�6G   $��1��6[   $��2��6h   5Q      5R      