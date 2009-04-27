Partial Public Class Page
    Inherits UserControl

    Private Shared window As Browser.HtmlWindow = Browser.HtmlPage.Window
    Private Shared document As Browser.HtmlDocument = Browser.HtmlPage.Document

    Public Sub New()
        InitializeComponent()

        Dim x As New Textarea
    End Sub

    Public Shared Sub log(ByVal x As Object)
        Dim console As Browser.ScriptObject = window.GetProperty("console")
        console.Invoke("log", x)
    End Sub

    Class Textarea
        Dim expression As String = Join(New String() { _
            "class A", _
            "   def m0", _
            "       'test method'", _
            "   end", _
            "   def m1(n)", _
            "       n * 2", _
            "   end", _
            "   def m2(n, m)", _
            "       n * m", _
            "   end", _
            "end", _
            "A.new" _
        }, vbCrLf)

        Dim document As Browser.HtmlDocument = Browser.HtmlPage.Document
        Dim div As Browser.HtmlElement
        Dim textarea As Browser.HtmlElement
        Dim button As Browser.HtmlElement
        Dim text As String = "<i>registeredName</i>.Invoke(<i>methodName</i>, <i>[args, ...]</i>)<br/>"

        Public Sub New()
            div = document.CreateElement("div")
            textarea = document.CreateElement("textarea")
            button = document.CreateElement("button")

            document.Body.AppendChild(div)
            div.SetProperty("innerHTML", text)
            div.AppendChild(textarea)
            div.AppendChild(button)
            textarea.SetProperty("value", expression)
            button.SetProperty("textContent", "set")
            button.AttachEvent("onclick", New EventHandler(Of Browser.HtmlEventArgs)(AddressOf Me.onclick))
        End Sub

        Public Sub onclick(ByVal o As Object, ByVal args As Browser.HtmlEventArgs)
            Dim expression As String = textarea.GetProperty("value")
            Dim obj As ScriptableRuby.RubyObject = ScriptableRuby.Create(expression)
            Dim name As String = window.Prompt("register name")
            Page.window.SetProperty(name, obj)
        End Sub
    End Class
End Class


Public Class ScriptableRuby
    Private Shared window As Browser.HtmlWindow = Browser.HtmlPage.Window
    Private Shared runtime As Microsoft.Scripting.Hosting.ScriptRuntime = IronRuby.Ruby.CreateRuntime
    Private Shared engine As Microsoft.Scripting.Hosting.ScriptEngine = runtime.GetEngine("IronRuby")
    Private Shared scope As Microsoft.Scripting.Hosting.ScriptScope = engine.CreateScope
    Private Shared operations As Microsoft.Scripting.Hosting.ObjectOperations = engine.CreateOperations(scope)

    Public Shared Function Create(ByVal source As String)
        Dim result = engine.Execute(source, scope)
        Return New RubyObject(ScriptableRuby.operations, result)
    End Function

    <Browser.ScriptableType()> _
    Public Class RubyObject
        Private result As Object

        Public Sub New(ByVal operations As Microsoft.Scripting.Hosting.ObjectOperations, ByVal result As Object)
            Me.result = result
        End Sub

        <Browser.ScriptableMember()> _
        Public Function Invoke(ByVal name As Object) As Object
            Return engine.Operations.Invoke(operations.GetMember(result, name))
        End Function

        <Browser.ScriptableMember()> _
        Public Function Invoke(ByVal name As Object, ByVal args As Browser.ScriptObject) As Object
            Dim length As Integer = args.GetProperty("length")
            Dim _args(length - 1) As Object

            For i As Integer = 0 To length - 1
                _args(i) = args.GetProperty(i)
            Next

            Return engine.Operations.Invoke(operations.GetMember(result, name), _args)
        End Function

        <Browser.ScriptableMember()> _
        Public Function Invoke(ByVal name As Object, ByVal ParamArray args As Object()) As Object
            Return engine.Operations.Invoke(operations.GetMember(result, name), args)
        End Function
    End Class
End Class
