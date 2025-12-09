using Avalonia.Controls;
using System;

namespace BankSystem.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Manejar cierre de ventana
        this.Closing += OnWindowClosing;
    }
    
    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Permitir cerrar la ventana sin problemas
        try
        {
            // Limpiar recursos si es necesario
        }
        catch (Exception)
        {
            // Ignorar errores al cerrar
        }
    }
}
