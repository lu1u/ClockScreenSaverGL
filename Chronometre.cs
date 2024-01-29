using System;

// Pour chronometrer une operation, et afficher le resultat sur la console
// Utilisation:
// using (var c = new Chronometre("message))
//  {
//  ... operation a chronometrer
//  }
namespace ClockScreenSaverGL
{
    internal class Chronometre : IDisposable
    {
#if DEBUG
                private readonly string _message;
                private readonly Stopwatch _chrono;
#endif
        public Chronometre(string message)
        {
#if DEBUG
                        _message = message;
                        _chrono = Stopwatch.StartNew();
                        Debug.Indent();
                        Debug.WriteLine("{");
#endif
        }

        public void Dispose()
        {
#if DEBUG
                        _chrono.Stop();
                        long l = _chrono.ElapsedMilliseconds;
                        Debug.WriteLine("}" + _message + " " + l + " millisecondes");
                        Debug.Unindent();
#endif
        }
    }
}
