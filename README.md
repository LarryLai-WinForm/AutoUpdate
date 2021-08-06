# AutoUpdate

echo $(TargetFileName) > TargetFileName

$(ProjectDir)$(OutDir)\AutoUpdate.Directory_Info_Data\AutoUpdate.Directory_Info_Data.exe $(ProjectDir)$(OutDir)

在參考AutoUpdate之專案中

進入屬性->建置事件->建置後事件命令列

輸入以上命令，說明如下

## 1. 自動更新執行檔設置
echo $(TargetFileName) > TargetFileName

可將執行檔名稱紀錄在TargetFileName檔案中，更新後若變更執行檔名也能順利更新

## 2. 自動更新用檔案清單-自動產出說明
$(ProjectDir)$(OutDir)\AutoUpdate.Directory_Info_Data\AutoUpdate.Directory_Info_Data.exe $(ProjectDir)$(OutDir)

建置完成時會自動寫入"JSON格式之檔案清單資料"到AutoUpdate.Directory_Info_Data.json檔案中

自動更新時會載入此資料進行作業