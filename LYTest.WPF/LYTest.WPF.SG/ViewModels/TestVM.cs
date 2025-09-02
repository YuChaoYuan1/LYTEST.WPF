using LYTest.ViewModel;
using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.WPF.SG.ViewModels
{
    internal class TestVM : ViewModelBase
    {

        public TestVM() { }

        public AsyncObservableCollection<CheckNodeViewModel> TreeSet { get; } = EquipmentData.CheckResults.Categories;


    }
}
