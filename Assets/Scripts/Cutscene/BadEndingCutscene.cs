using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Cutscenes
{
    public static Cutscene BadEnding
    {
        get
        {
            return new Cutscene() {
                new ShowDialogAction(true),
                new DialogAction("Perjalananmu di Blade Farm 2045 sudah selesai... dengan kesedihan."),
                new DialogAction("Apartemen Abdul kini sepi sekali. Tanaman-tanaman yang dulu ditanam penuh harapan, sekarang layu dan mati satu per satu."),
                new DialogAction("Hutang 1000 kredit ke Bank Riba Ilegal itu tidak bisa dibayar. Waktu habis, dan harapan pun ikut hilang."),
                new DialogAction("Usaha 'real food' mereka tidak berhasil. Orang-orang sudah terbiasa dengan makanan buatan yang tidak ada rasanya."),
                new DialogAction("'THE ONE' masih sangat berkuasa. Gedung-gedung tinggi mereka seperti mengejek, menunjukkan bahwa mereka tidak bisa dikalahkan."),
                new DialogAction("Di tengah kota futuristik yang ramai, Abdul dan istrinya kembali menjalani hidup yang suram."),
                new DialogAction("Mereka makan 'Maximum Processed Food' yang hambar. Tanpa selera, tanpa semangat. Mimpi 'real food' sekarang hanya jadi bisikan sedih di malam hari."),
                new DialogAction("Hati mereka rasanya hancur. Bukan cuma impian yang mati, tapi semangat untuk berjuang juga hilang."),
                new DialogAction("Terkadang, kegagalan bukan hanya mengakhiri sebuah usaha, tapi juga menghancurkan jiwa."),
                new DialogAction("Terima kasih sudah bermain Blade Farm 2045. Semoga kamu menemukan kebahagiaan di tempat lain."),
                new ShowDialogAction(false),
                new WaitAction(2f)
            };
        }
    }
}