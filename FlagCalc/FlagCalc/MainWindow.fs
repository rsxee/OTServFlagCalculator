﻿open System
open System.Windows
open System.Windows.Controls
open System.Text.RegularExpressions
open Microsoft.Win32;
open FSharpx
 
type MainWindow = XAML<"MainWindow.xaml">
 
let loadWindow() =
    let window = MainWindow()

    let loadFlags (filePath : string) =
        let addItem name =
            let item = ListBoxItem()
            item.Content <- name
            item.ToolTip <- pown 2UL  window.FlagList.Items.Count
            window.FlagList.Items.Add item 
            |> ignore

        let formatText (text : string) =
            let splitCamelCase str = 
                Regex.Replace(
                    Regex.Replace(
                        str, 
                        @"(\P{Ll})(\P{Ll}\p{Ll})", 
                        "$1 $2" 
                    ),
                    @"(\p{Ll})(\P{Ll})", "$1 $2"
                )

            let commaPosition = text.IndexOf ','
            let whitespacePosition = text.IndexOf ' '
            let selectTo = 
                if commaPosition > whitespacePosition then 
                    whitespacePosition 
                else 
                    commaPosition
            let selectFrom = 11 // skips 'PlayerFlag_'

            text.[selectFrom .. selectTo - 1] |> splitCamelCase

        System.IO.File.ReadLines(filePath)
        |> Seq.filter (fun x -> x.Contains "PlayerFlag_" && not (x.Contains "LastFlag"))
        |> Seq.map (fun x -> formatText (x.Trim()))
        |> Seq.iter (fun x -> addItem x)

        if window.FlagList.Items.Count = 0 then
            MessageBox.Show ("No flags found", "Error", MessageBoxButton.OK) |> ignore

    window.FlagList.SelectionChanged.Add(fun _ -> 
        let mutable flags = 0UL
        for item in window.FlagList.SelectedItems do
            flags <- flags + (pown 2UL (window.FlagList.Items.IndexOf item))
        window.TextCalc.Text <- flags.ToString()
    )

    window.ClearButton.Click.Add(fun _ -> window.FlagList.SelectedIndex <- -1)

    window.OpenFile.Click.Add(fun _ ->
        window.FlagList.Items.Clear()
        let fileDialog = OpenFileDialog()
        fileDialog.ShowDialog() 
        |> fun result -> 
            if result.HasValue && result.Value = true then 
                loadFlags fileDialog.FileName |> ignore
    )

    window.Exit.Click.Add(fun _ -> window.Root.Close())

    window.Root
 
[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore