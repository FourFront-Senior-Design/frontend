﻿using DataStructures;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ViewModelInterfaces;
using ServicesInterfaces;
using Common;
using System;

namespace ViewModels
{
    public class ReviewWindowVM : IReviewWindowVM, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler HeadstoneChanged;

        private int _currentPageIndex;
        private Headstone _currentPageData;
        private IDatabaseService _database;
        private string _prevCemeteryName = "";
        private string _prevSectionNumber = "";
        private string _prevMarkerType = "";

        public Headstone CurrentPageData
        {
            get
            {
                return _currentPageData;
            }
            set
            {
                _currentPageData = value;
                _currentPageData.Prev1GravesiteNum = _database.GetGraveSiteNum(PageIndex - 1);
                _currentPageData.Prev2GravesiteNum = _database.GetGraveSiteNum(PageIndex - 2);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPageData)));
            }
        }

        // 1-indexed because the MS Access database is 1-indexed
        public int PageIndex 
        {
            get
            {
                return _currentPageIndex;
            }
            set
            {
                if (_currentPageIndex != 0)
                {
                    _database.SetHeadstone(_currentPageIndex, _currentPageData);
                }
                _currentPageIndex = value;
                displayHeadStone();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PageIndex)));
            }
        }

        public string Title
        {
            get
            {
                string _version = System.Reflection.AssemblyName.GetAssemblyName("DataEntryForm.exe").Version.ToString();
                return $"Review (Verison {_version})";
            }
        }

        public int GetDatabaseCount
        {
            get
            {
                return _database.TotalItems;
            }
        }
        
        public string ImageSource1
        {
            get
            {
                return _database.SectionFilePath + "\\" + Constants.REFERENCED_IMAGE_FOLDER_NAME + 
                    "\\" + CurrentPageData.Image1FileName;
            }
        }

        public string ImageSource2
        {
            get
            {
                return _database.SectionFilePath + "\\" + Constants.REFERENCED_IMAGE_FOLDER_NAME + 
                    "\\" + CurrentPageData.Image2FileName;
            }
        }
        
        public ReviewWindowVM(IDatabaseService database)
        {
            _database = database;
        }

        private void displayHeadStone()
        {
            CurrentPageData = _database.GetHeadstone(_currentPageIndex);
            HeadstoneChanged?.Invoke(this, new EventArgs());
        }

        public void NextRecord()
        {
            if (PageIndex == _database.TotalItems)
            {
                return;
            }

            /* copies the information forward on clicking the next record*/
            _prevCemeteryName = _currentPageData.CemeteryName;
            _prevSectionNumber = _currentPageData.BurialSectionNumber;
            _prevMarkerType = _currentPageData.MarkerType;

            PageIndex++;
            

            if (string.IsNullOrEmpty(CurrentPageData.CemeteryName))
            {
                CurrentPageData.CemeteryName = _prevCemeteryName;
            }

            if (string.IsNullOrEmpty(CurrentPageData.BurialSectionNumber))
            {
                CurrentPageData.BurialSectionNumber = _prevSectionNumber;
            }

            if (string.IsNullOrEmpty(CurrentPageData.MarkerType))
            {
                CurrentPageData.MarkerType = _prevMarkerType;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPageData)));
        }
        
        public void PreviousRecord()
        {
            if (PageIndex == 1)
            {
                return;
            }

            PageIndex--;
        }

        public void FirstRecord()
        {

            PageIndex = 1;
        }

        public void LastRecord()
        {

            PageIndex = _database.TotalItems;
        }

        public bool GoToRecord(string text)
        {
            try
            {
                int page = Convert.ToInt32(text);
                if (page < 1) return false;
                PageIndex = page;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<EmblemData> GetEmblemData
        {
            get
            {
                return new EmblemData().GetEmblemData;
            }
        }  

        public List<CemeteryNameData> GetCemeteryNames
        {
            get
            {
                return _database.CemeteryNames;
            }
        }

        public List<LocationData> GetLocationData
        {
            get
            {
                return _database.LocationNames;
            }
        }

        public List<BranchData> GetBranchData
        {
            get
            {
                return _database.BranchNames;
            }
        }

        public List<WarData> GetWarData
        {
            get
            {
                return _database.WarNames;
            }
        }

        public List<AwardData> GetAwardData
        {
            get
            {
                return _database.AwardNames;
            }
        }

        public List<bool> CheckMandatoryFields()
        {
            List<bool> isValidList = new List<bool>()
            {
                true, // cemetery Name
                true, // Marker Type
                true, // emblem 1
                true, // burial Section
                true, // wall ID
                true, // row Number
                true, // gravesite number
                true, // primary last name
                true, // descedent 1 last name
                true, // descedent 2 last name
                true, // descedent 3 last name
                true, // descedent 4 last name
                true, // descedent 5 last name
                true  // descedent 6 last name
            };

            if (String.IsNullOrEmpty(_currentPageData.CemeteryName))
            {
                isValidList[0] = false;
            }

            if (String.IsNullOrEmpty(_currentPageData.MarkerType))
            {
                isValidList[1] = false;
            }

            if (String.IsNullOrEmpty(_currentPageData.Emblem1))
            {
                isValidList[2] = false;
            }

            if (String.IsNullOrEmpty(_currentPageData.BurialSectionNumber))
            {
                isValidList[3] = false;
            }

            if (String.IsNullOrEmpty(_currentPageData.WallID))
            {
                isValidList[4] = false;
            }

            if (String.IsNullOrEmpty(_currentPageData.RowNum))
            {
                isValidList[5] = false;
            }

            if (String.IsNullOrEmpty(_currentPageData.GavestoneNumber))
            {
                isValidList[6] = false;
            }

            if (String.IsNullOrEmpty(_currentPageData.PrimaryDecedent.LastName))
            {
                isValidList[7] = false;
            }

            int personIndex = 8;
            foreach (Person person in _currentPageData.OthersDecedentList)
            {
                // A person with contents is valid iff they have a last name
                if (person.containsData() && String.IsNullOrEmpty(person.LastName))
                {
                    isValidList[personIndex] = false;
                }

                personIndex += 1;
            }

            return isValidList;
        }

        public void SaveRecord()
        {
            if (_currentPageIndex != 0)
            {
                _database.SetHeadstone(_currentPageIndex, _currentPageData);
            }
        }
        
        public void CloseDatabase()
        {
            _database.Close();
        }
    }
}
