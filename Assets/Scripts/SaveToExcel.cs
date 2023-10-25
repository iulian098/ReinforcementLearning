using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public static class SaveToExcel
{
    static IWorkbook workbook;

    public static bool Save(string path) {

        using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write)) {
            workbook.Write(stream);
        }

        return true;
    }

    public static void CreateWorkbook() {
        workbook = new HSSFWorkbook();
    }

    public static bool AddSheet(string sheetName = "") {

        if (sheetName == "")
            if (workbook.GetSheet("data") == null)
                workbook.CreateSheet("data");
        else
            if (workbook.GetSheet(sheetName) == null)
                workbook.CreateSheet(sheetName);

        return true;
    }

    public static void AddData(float[] data, int rowIndex, string sheetName = "" ) {
        ISheet sheet = workbook.GetSheet("data");
        IRow row = sheet.CreateRow(rowIndex);

        for (int i = 0; i < data.Length; i++) {
            ICell cell = row.CreateCell(i);
            cell.SetCellValue(data[i]);
        }
    }

}
