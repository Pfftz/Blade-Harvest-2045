using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Cutscenes
{
    public static Cutscene IntroCutscene
    {
        get
        {
            return new Cutscene() {
                new ShowDialogAction(true),
                new DialogAction("Jakarta, 2045. Kota ini dikuasai 'THE ONE,' perusahaan makanan raksasa yang hanya menyisakan 'Maximum Processed Food'—biji-bijian hambar pengganti 'real food.'"),
                new DialogAction("Abdul dan istrinya hidup dalam rutinitas monoton, menelan makanan tanpa rasa, di apartemen futuristik yang suram."),
                new DialogAction("Masalah Abdul lebih dari sekadar makanan. Ia terjerat hutang warisan 1000 kredit pada 'Bank Riba Ilegal.'"),
                new DialogAction("Gajinya sebagai buruh 'THE ONE' nyaris tak cukup. Bagaimana mungkin ia melunasi hutang ini di dunia yang hampa rasa? Keputusasaan mulai mencekik."),
                new DialogAction("Namun, di tengah kemonotonan itu, sepercik ide gila melintas di benak Abdul. Membangkitkan 'real food'!"),
                new DialogAction("Tomat segar, mentimun renyah, padi, kubis—rasa yang terlupakan! Sebuah ambisi nekat di era tanpa keaslian."),
                new DialogAction("Dengan tekad membara, Abdul mengubah apartemennya jadi kebun rahasia. Istrinya, dengan 'Resep Nenek' di tangan, mulai memasak hidangan 'Pecel' yang legendaris."),
                new DialogAction("Mereka melawan dominasi 'THE ONE,' satu per satu sayuran, satu per satu hidangan. Bisakah mereka memanen keberanian dan rasa di dunia yang hambar?"),
                
                // --- Bagian instruksi game ---
                // Activate the Player object that's already on the canvas
                new ActivateObjectAction("Player"),
                new DialogAction("Gunakan alatmu bijak - setiap aksi butuh stamina."),
                new DialogAction("Istirahat di ranjang setiap malam untuk pulihkan energi dan lanjut ke hari berikutnya."),
                new DialogAction("Semoga berhasil, petani. Masa depan pangan ada di tanganmu!"),
                new ShowDialogAction(false),
                new WaitAction(1f),
                // Deactivate the Player object - Ini kemungkinan besar tidak diperlukan jika player sudah diaktifkan dan siap dimainkan.
                new DeactivateObjectAction("Player") 
            };
        }
    }
}