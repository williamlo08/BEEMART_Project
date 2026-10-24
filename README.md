BEEMART

Aplikasi Point of Sales (POS) berbasis web yang dirancang untuk memenuhi kebutuhan kasir dan admin dalam mengelola transaksi penjualan secara efisien. Project ini dibangun menggunakan arsitektur **Client-Server** di mana Frontend (HTML/JS) berkomunikasi dengan Backend (ASP.NET Core API) melalui HTTP Request.

# Teknologi yang Digunakan

Frontend:
*   HTML5, CSS3, Vanilla JavaScript
*   Disajikan menggunakan Live Server

Backend:
*   ASP.NET Core Web API (.NET 8)
*   Entity Framework (EF) Core
*   BCrypt

Database:
*   SQL Server (Express/LocalDB)

---

# Fitur Utama

1. Autentikasi & Otorisasi (Login/Register)
*   **Registrasi Akun:** Mendukung pendaftaran user baru dengan role `Admin` atau `Kasir`. Validasi username unik (tidak boleh sama).
*   **Keamanan Password:** Password di-hash menggunakan algoritma **BCrypt** sebelum disimpan ke database. Tidak ada *plain text*.
*   **Login:** Verifikasi password menggunakan metode hashing. Menolak akses jika kredensial salah (Invalid login).
*   **Session Management:** Menyimpan state login di `localStorage` dan mengarahkan user sesuai role.

2. Transaksi Kasir (Cashier)
*   **Pencarian Barang:** Pencarian produk secara *real-time* via API (maksimal 10 hasil).
*   **Keranjang Belanja:** Menambah produk ke keranjang, mengatur QTY, dan menghapus item.
*   **Perhitungan Otomatis:** Subtotal dan Total Harga dihitung otomatis, termasuk diskon per item.
*   **Generate Invoice:** Pembuatan nomor invoice otomatis dengan format baku: `[5 digit userID][ddmmyy][3 digit running number]` (contoh: `U0001231026001`).
*   **Metode Pembayaran:** Mendukung Cash, Debit, dan Credit (diambil dari API).

3. History Transaksi & Hak Akses
*   **Kasir:** Hanya dapat melihat riwayat transaksi miliknya sendiri.
*   **Admin:** Dapat melihat seluruh riwayat transaksi dari semua kasir.
*   **Pagination:** Riwayat transaksi dibatasi maksimal 20 data per halaman dengan UI navigasi halaman.
*   **Filter Tanggal:** Menyaring riwayat berdasarkan tanggal tertentu.

---

## Cara Menjalankan Project (Local Development)

# Software dibutuhkan :
1.  [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2.  [SQL Server Express / LocalDB](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
3.  [Visual Studio Code](https://code.visualstudio.com/) + Ekstensi **Live Server**
4.  [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

Langkah 1: Setup Database
1.  Buka SSMS dan buat database baru dengan nama `BeeMartDb`.
2.  Jalankan script SQL yang disediakan (Create Table + Insert Data).
3.  Pastikan data sudah masuk (misal: `SELECT COUNT(*) FROM MsProduct;`).

Langkah 2: Jalankan Backend API
1.  Buka terminal di VS Code.
2.  Masuk ke folder API: `cd BeeMart.Api`
3.  Sesuaikan connection string di `appsettings.json` jika nama server SQL Anda berbeda.
4.  Jalankan aplikasi:
    ```bash
    dotnet run
