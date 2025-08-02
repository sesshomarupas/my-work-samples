#In[] Import necessary libraries
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from statistics import mean 
from statistics import stdev
import seaborn as sn
import os
import openpyxl

# Generating data from excel sheets
pathway_dataSet = "C:\\Users\\pastel\\Desktop\\Erstellung Artikel\\Enhancing adaptations to peripheral distractors in real world during basketball throwing through virtual reality application\\Videos\\"
list_excelData = []

# Saving all excel sheets 
for entry in os.listdir(pathway_dataSet):
    list_excelData.append(entry)

# 0 for data of RW; 1 for data of VR; 2 for data of RW without avatar
Results_Group_RealWorld_WO = pd.DataFrame(); Results_Group_VirtualReality_WO = pd.DataFrame(); Results_Group_RealWorld_WithoutO = pd.DataFrame()
group = int(input("Drücke die 0 für RW; 1 für RW_2; 2 für VR"))
data = pd.read_excel(pathway_dataSet + list_excelData[group])

# list all sheets of the excel file
def read_excel_sheets(filepath):
        list_sheetNames = []
        workbook = openpyxl.load_workbook(filepath)
        sheet_names = workbook.sheetnames
        for sheet_name in sheet_names:
            #print("SheetName: {sheet_name}")
            list_sheetNames.append(sheet_name)
        return list_sheetNames

# Iterating through the sheets and calculating the mean and standard deviation 
file_path = pathway_dataSet + list_excelData[group]
sheet_names = read_excel_sheets(file_path); del(sheet_names[0]) # just inlcuding the sheet of each individual
number_of_sheet = 0

# Lists of each participants' performances
pretestWithoutAvatar = []; pretestWithAvatarLeft = []; pretestWithAvatarMid = []; pretestWithAvatarRight = []
posttestWithoutAvatar = []; posttestWithAvatarLeft = []; posttestWithAvatarMid = []; posttestWithAvatarRight = []

# Development of the goal dictionary including all means of scores
dic_MeanDataset = {"Pretest_WithoutAvatar": [], "Pretest_WithAvatarLeft": [], "Pretest_WithAvatarMid": [], "Pretest_WithAvatarRight": [], 
                   "Posttest_WithoutAvatar": [], "Posttest_WithAvatarLeft": [], "Posttest_WithAvatarMid": [], "Posttest_WithAvatarRight": []}

def calculatingMeans(dic_MeanDataset, number_of_sheet):

    dataSetSheet = pd.DataFrame(pd.read_excel(file_path, sheet_name=sheet_names[number_of_sheet]))
    pretestWithoutAvatar = dataSetSheet.iloc[:15, 1]; pretestWithAvatarLeft = dataSetSheet.iloc[:15, 2]; pretestWithAvatarMid = dataSetSheet.iloc[:15, 3]; pretestWithAvatarRight = dataSetSheet.iloc[:15, 4]
    posttestWithoutAvatar = dataSetSheet.iloc[:15, 6]; posttestWithAvatarLeft = dataSetSheet.iloc[:15, 7]; posttestWithAvatarMid = dataSetSheet.iloc[:15, 8]; posttestWithAvatarRight = dataSetSheet.iloc[:15, 9]
    # Lists of the means of each conditions
    Means_Pre_WithoutAvatar = mean(pretestWithoutAvatar); Means_Pre_WithAvatarLeft = mean(pretestWithAvatarLeft); Means_Pre_WithAvatarMid = mean(pretestWithAvatarMid); Means_Pre_WithAvatarRight = mean(pretestWithAvatarRight)
    Means_Post_WithoutAvatar = mean(posttestWithoutAvatar); Means_Post_WithAvatarLeft = mean(posttestWithAvatarLeft); Means_Post_WithAvatarMid = mean(posttestWithAvatarMid); Means_Post_WithAvatarRight = mean(posttestWithAvatarRight)

    ListOfMeans_Pretest = [round(Means_Pre_WithoutAvatar, 2), round(Means_Pre_WithAvatarLeft, 2), round(Means_Pre_WithAvatarMid, 2), round(Means_Pre_WithAvatarRight, 2)]
    ListOfMeans_Posttest = [round(Means_Post_WithoutAvatar, 2), round(Means_Post_WithAvatarLeft, 2), round(Means_Post_WithAvatarMid, 2), round(Means_Post_WithAvatarRight, 2)]
    # Add values to dictionary
    dic_MeanDataset["Pretest_WithoutAvatar"].append(ListOfMeans_Pretest[0]); dic_MeanDataset["Pretest_WithAvatarLeft"].append(ListOfMeans_Pretest[1]); dic_MeanDataset["Pretest_WithAvatarMid"].append(ListOfMeans_Pretest[2]); dic_MeanDataset["Pretest_WithAvatarRight"].append(ListOfMeans_Pretest[3])
    dic_MeanDataset["Posttest_WithoutAvatar"].append(ListOfMeans_Posttest[0]); dic_MeanDataset["Posttest_WithAvatarLeft"].append(ListOfMeans_Posttest[1]); dic_MeanDataset["Posttest_WithAvatarMid"].append(ListOfMeans_Posttest[2]); dic_MeanDataset["Posttest_WithAvatarRight"].append(ListOfMeans_Posttest[3])

    return dic_MeanDataset

dic_MeanDataset = calculatingMeans(dic_MeanDataset, number_of_sheet)

for i in range(len(sheet_names) - 1):
     number_of_sheet +=1
     dic_MeanDataset = calculatingMeans(dic_MeanDataset, number_of_sheet)

if  (group == 0): 
    Results_Group_RealWorld_WO = pd.DataFrame(dic_MeanDataset)
    Results_Group_RealWorld_WO.to_csv("RW_WithOpponent.csv")
elif (group == 1):
    Results_Group_RealWorld_WithoutO = pd.DataFrame(dic_MeanDataset)
    Results_Group_RealWorld_WithoutO.to_csv("RW_WithoutOpponent.csv")
else:
    Results_Group_VirtualReality_WO = pd.DataFrame(dic_MeanDataset)
    Results_Group_VirtualReality_WO.to_csv("VR_WithOpponent.csv")

#In[] Next steps calculating the mean and std of all participants; graphics
import os
import pandas as pd

pathway_excel =  "C:\\Users\\pastel\\Desktop\\Erstellung Artikel\\Enhancing adaptations to peripheral distractors in real world during basketball throwing through virtual reality application\\Auswertung"; listExcel = []

for i in os.listdir(pathway_excel):
    listExcel.append(i)

# global functions
def filteredCSVList(string_list): # filtering only the csv files
     strings_csv = [i for i in string_list if i.endswith(".csv")]
     return strings_csv

def firstToEnd(importedList): # putting the first element at the end
     firstElement = importedList.pop(0)
     importedList.append(firstElement)

# values for between-subject factor
def fillingGroupClarification(x, y, z):
    return [1] * len(x) + [2] * len(y) + [3] * len(z)

listExcel = filteredCSVList(listExcel)
firstToEnd(listExcel)

def descriptiveNames(x, excelList):
    name_list = []
    for i in x:
        if i == 1:
            name_list.append(excelList[0][:-4])
        elif i == 2:
            name_list.append(excelList[1][:-4])
        else:
            name_list.append(excelList[2][:-4])
    return name_list
#In[]
# group performances
groupRW = pd.read_csv(pathway_excel+ "\\" +listExcel[0]); groupVR = pd.read_csv(pathway_excel+ "\\" +listExcel[1]); groupRW_2 = pd.read_csv(pathway_excel+ "\\" +listExcel[2])
between_subject = fillingGroupClarification(groupRW, groupRW_2, groupVR)
descriptiveNameList = descriptiveNames(between_subject, excelList = listExcel)

wholeDataset = pd.DataFrame({"Group": between_subject, "Names": descriptiveNameList,"Pretest_WithoutAvatar": list(groupRW["Pretest_WithoutAvatar"]) + list(groupRW_2["Pretest_WithoutAvatar"]) + list(groupVR["Pretest_WithoutAvatar"]),
                             "Pretest_WithAvatarLeft": list(groupRW["Pretest_WithAvatarLeft"]) + list(groupRW_2["Pretest_WithAvatarLeft"]) + list(groupVR["Pretest_WithAvatarLeft"]),
                             "Pretest_WithAvatarMid": list(groupRW["Pretest_WithAvatarMid"]) + list(groupRW_2["Pretest_WithAvatarMid"]) + list(groupVR["Pretest_WithAvatarMid"]),
                             "Pretest_WithAvatarRight": list(groupRW["Pretest_WithAvatarRight"]) + list(groupRW_2["Pretest_WithAvatarRight"]) + list(groupVR["Pretest_WithAvatarRight"]),
                             "Posttest_WithoutAvatar": list(groupRW["Posttest_WithoutAvatar"]) + list(groupRW_2["Posttest_WithoutAvatar"]) + list(groupVR["Posttest_WithoutAvatar"]),
                             "Posttest_WithAvatarLeft": list(groupRW["Posttest_WithAvatarLeft"]) + list(groupRW_2["Posttest_WithAvatarLeft"]) + list(groupVR["Posttest_WithAvatarLeft"]),
                             "Posttest_WithAvatarMid": list(groupRW["Posttest_WithAvatarMid"]) + list(groupRW_2["Posttest_WithAvatarMid"]) + list(groupVR["Posttest_WithAvatarMid"]),
                             "Posttest_WithAvatarRight": list(groupRW["Posttest_WithAvatarRight"]) + list(groupRW_2["Posttest_WithAvatarRight"]) + list(groupVR["Posttest_WithAvatarRight"])})

wholeDataset.to_csv("C:\\Users\\pastel\\Desktop\\Erstellung Artikel\\Enhancing adaptations to peripheral distractors in real world during basketball throwing through virtual reality application\\Auswertung\\GanzerDatensatz\\wholeDataSet.csv")

#In[] Outlier detection
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sn
import openpyxl
from statistics import mean
from statistics import median
from statistics import stdev
from statsmodels import robust

data = pd.DataFrame(pd.read_csv("C:\\Users\\pastel\\Desktop\\Erstellung Artikel\\Enhancing adaptations to peripheral distractors in real world during basketball throwing through virtual reality application\\Auswertung\\GanzerDatensatz\\wholeDataSet.csv"))

# Outlierdetection for the pretest data
dic_pretest = pd.DataFrame({"WithoutAvatar": data["Pretest_WithoutAvatar"], "WithAvatarLeft" : data["Pretest_WithAvatarLeft"], "WithAvatarMid" : data["Pretest_WithAvatarMid"], "WithAvatarRight" : data["Pretest_WithAvatarRight"]})

relativeEffect_Left = []; relativeEffect_Mid = []; relativeEffect_Right = []

def relativeValuesCalculation(x,y,listRV):
    for i in range(dic_pretest.shape[0]):
        relativeValue = y[i]/x[i]
        listRV.append(relativeValue)
    return listRV

relativeValuesCalculation(dic_pretest["WithoutAvatar"], dic_pretest["WithAvatarLeft"], relativeEffect_Left); relativeValuesCalculation(dic_pretest["WithoutAvatar"], dic_pretest["WithAvatarMid"], relativeEffect_Mid); relativeValuesCalculation(dic_pretest["WithoutAvatar"], dic_pretest["WithAvatarRight"], relativeEffect_Right)

dic_outliers = pd.DataFrame({"": data["Group"], "Outliers_Left": relativeEffect_Left, "Outliers_Mid": relativeEffect_Mid, "Outliers_Right": relativeEffect_Right})
dic_outliers.to_csv("Outliers.csv")
IndexOutliersLeft = []; IndexOutliersMid = []; IndexOutliersRight = []

def columnOutliersIdentification(data, list):
    values = []
    data_median = median(data); data_mad = robust.mad(data)
    borderUp = data_median + 3 * data_mad; borderDown = data_median - 3 * data_mad

    for index, element in enumerate(data):
        if element > borderUp or element < borderDown:
            list.append(index)
        else:
            values.append(element)

    return list, element

columnOutliersIdentification(dic_pretest["WithAvatarLeft"], IndexOutliersLeft)
columnOutliersIdentification(dic_pretest["WithAvatarMid"], IndexOutliersMid)
columnOutliersIdentification(dic_pretest["WithAvatarRight"], IndexOutliersRight)

IndexOfOutliers = [IndexOutliersLeft, IndexOutliersMid, IndexOutliersRight]

def recognizingOutliers(ListOutliers):
    newList = []
    for i in ListOutliers:
        for y in i:
            if y not in newList:
                newList.append(y)
    return newList

IndexOfOutliers = recognizingOutliers(IndexOfOutliers); print(IndexOfOutliers)

# Data without outliers 
data_without_outliers = data.drop(index=IndexOfOutliers)
data_without_outliers.to_csv("C:\\Users\\pastel\\Desktop\\Erstellung Artikel\\Enhancing adaptations to peripheral distractors in real world during basketball throwing through virtual reality application\\Auswertung\\GanzerDatensatz\\DataWithoutOutliers.csv")


data_visualization = data_without_outliers.iloc[:, 3:7]
data_visualization = data_visualization.rename(columns={"Pretest_WithoutAvatar" : "WOO", "Pretest_WithAvatarLeft" : "WL", "Pretest_WithAvatarMid": "WM","Pretest_WithAvatarRight": "WR"})

means = data_visualization.mean(); stds = data_visualization.std()

fig, ax = plt.subplots()
x = np.arange(len(data_visualization.columns))
width = 0.4
colors = ["blue", "orange", "orange", "orange"]

for i, (mean, std, color) in enumerate(zip(means, stds, colors)):
    ax.bar(x[i], mean, width, yerr=std, capsize=4, color=color)
ax.set_ylim(0.6, ax.get_ylim()[1]) # untere und obere Grenze
ax.set_xlabel('Conditions')
ax.set_ylabel('Average Points from the Scoring System')
ax.set_title('Results of the Pretest between the Conditions')
ax.set_xticks(x)
ax.set_xticklabels(data_visualization.columns, fontsize = 10)
plt.tight_layout()
plt.savefig("C:\\Users\\pastel\\Desktop\\Erstellung Artikel\\Enhancing adaptations to peripheral distractors in real world during basketball throwing through virtual reality application\\Auswertung\\Abbildungen\\Pretest.png", dpi = 300)
#In[] Detect outliers for the 'WithAvatarRight' column from the 21st element onward
indices_outliers_right, values_without_outliers_right = columnOutliersIdentification(dic_pretest["WithAvatarRight"][20:])

print("Indices of outliers:", indices_outliers_right)
print("Values without outliers:", values_without_outliers_right)

# Visualize the data before and after removing outliers
plt.figure(figsize=(12, 6))

plt.subplot(1, 2, 1)
plt.title("WithAvatarRight - Original Data")
sn.boxplot(dic_pretest["WithAvatarRight"][20:])

plt.subplot(1, 2, 2)
plt.title("WithAvatarRight - Without Outliers")
sn.boxplot(values_without_outliers_right)

plt.show()
# %%
