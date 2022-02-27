/* 
 * This file is part of the AutoDisplayRotateApp distribution (https://github.com/DarthAlex77/DisplayRotate).
 * Copyright (c) 2022 Aleksey Surgaev.
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity;
using Syncfusion.UI.Xaml.Grid;

namespace DisplayRotate.Extras
{
    public class SelectionBehaviour : Behavior<SfDataGrid>
    {
        public static readonly DependencyProperty FirstDetailsViewSelectedItemProperty =
        DependencyProperty.Register("FirstDetailsViewSelectedItem", typeof(object), typeof(SelectionBehaviour), new PropertyMetadata(null));

        public static readonly DependencyProperty ViewCurrentItemProperty =
        DependencyProperty.Register("ViewCurrentItem", typeof(object), typeof(SelectionBehaviour), new PropertyMetadata(null));

        public object FirstDetailsViewSelectedItem
        {
            get => GetValue(FirstDetailsViewSelectedItemProperty);
            set => SetValue(FirstDetailsViewSelectedItemProperty, value);
        }

        public object ViewCurrentItem
        {
            get => GetValue(ViewCurrentItemProperty);
            set => SetValue(ViewCurrentItemProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            ((GridViewDefinition) AssociatedObject.DetailsViewDefinition[0]).DataGrid.SelectionChanged += FirstDetailsViewDataGridSelectionChanged;

            AssociatedObject.View.CurrentChanged += View_CurrentChanged;
        }

        private void View_CurrentChanged(object sender, EventArgs e)
        {
            ViewCurrentItem = AssociatedObject.View.CurrentItem;
        }

        private void FirstDetailsViewDataGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            FirstDetailsViewSelectedItem = ((SfDataGrid) e.OriginalSender).SelectedItem;
        }
    }
}