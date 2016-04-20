namespace App1
{
    using System;
    using SQLite.Net.Attributes;
    using Repository.Interfaces;

    public enum StatusDownload
    {
        InProgress = 1,
        Paused,
        Finished,
        Error
    }

    [Table("Download")]
    public class Download : IDomainEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public StatusDownload StatusDownloadKey { get; set; } // INTEGER, es el estado de la descarga

        public string StreamType { get; set; } // TEXT, // Stream type, "est_hd", "est_sd" or "smooth_streaming" for ss caching.

        public int QualityBitrate { get; set; } // INTEGER, // The configured bitrate for the selected qualty

        public DateTime AcquisitionDate { get; set; } 

        public DateTime? StartDownloadDate { get; set; } 

        public DateTime? ExpirationDownloadDate { get; set; } 

        public byte[] Image { get; set; } // BINARY // Thumbnail en la lista de descargas
        
        public byte[] ImagePortrait { get; set; } // BINARY // Thumbnail en la lista de descargas

        public string Title { get; set; } // TEXT, // se muestra en la pantalla (como en la ficha ej. 'Wall-e')

        public string LanguageDescriptionSmall { get; set; } // TEXT, // se muestra en la pantalla (como en la ficha ej. 'Doblada al Español')

        public string LanguageDescriptionLarge { get; set; } // TEXT, // se muestra en la pantalla (como en la ficha ej. 'Doblada al Español')

        public long FileSize { get; set; } // NUMERIC, // tamaño del archivo bytes o kbytes (ver chuncks) ???

        public long ProgresDownloaded { get; set; } // TEXT, // tamaño descargado del archivo bytes o kbytes (ver chuncks) ???

        public string VideoUrl { get; set; } // TEXT, // URL for the download.  This will be an SS url for Desktop, and a .ismv file with audio and video track for devices like iOS/Android.

        public string PhysicalPath { get; set; } // Ruta de la descarga fisica del archivo

        public bool LicenseAcquired { get; set; } // Si ya comenzo la reproduccion del video

        public bool NotifyRecentDownload { get; set; } // Indica si se debe notificar la descarga reciente del archivo.

        public Download()
        {
            AcquisitionDate = DateTime.Now;
            ExpirationDownloadDate = AcquisitionDate.AddMonths(1);
            StreamType = "";
            Title = "";
            LanguageDescriptionSmall = "";
            LanguageDescriptionLarge = "";
            VideoUrl = "";
            PhysicalPath = "";
            StatusDownloadKey = StatusDownload.InProgress;
            ProgresDownloaded = 0;
            LicenseAcquired = false;
        }
    }
}
