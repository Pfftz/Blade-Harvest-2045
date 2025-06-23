using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Cutscenes
{
    public static Cutscene GoodEnding
    {
        get
        {
            return new Cutscene() {
                new ShowDialogAction(true),
                new DialogAction("Selamat!!! Kamu telah mencapai akhir yang baik!"),
                new DialogAction("Apartemen Abdul kini subur, penuh tanaman segar. Bersama istrinya, mereka tersenyum lega karena hutang 1000 kredit lunas!"),
                new DialogAction("Toko mereka meledak laris. Antrean pembeli mengular, haus akan rasa yang otentik. Abdul telah sukses besar!"),
                new DialogAction("Namun, di markas 'THE ONE', alarm berbunyi. Penjualan 'Maximum Processed Food' merosot. Sebuah toko pesaing telah muncul di Jakarta."),
                new DialogAction("CEO 'THE ONE' menatap layar, senyum berbahaya tersungging. 'Sepertinya ada semut yang mencoba merusak pasar kita,' gumamnya dingin."),
                new DialogAction("Perjuangan belum usai, namun Abdul kini kuat. Masa depan pangan telah berubah, dan 'real food' kembali menancapkan akarnya di 2045!"),
                new DialogAction("Terima kasih telah bermain Blade Farm 2045!"),
                new ShowDialogAction(false),
                new WaitAction(2f)
            };
        }
    }
}