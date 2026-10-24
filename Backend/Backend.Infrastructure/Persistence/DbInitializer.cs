using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Infrastructure.Persistence;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.Brands.Any()) return; // уже засеяно

        // ─── Бренды ──────────────────────────────────
        var apple = new Brand("Apple"); 
        var samsung = new Brand("Samsung");
        var sony = new Brand("Sony");
        var xiaomi = new Brand("Xiaomi");
        var asus = new Brand("ASUS");
        var lenovo = new Brand("Lenovo");
        var hp = new Brand("HP");
        var dell = new Brand("Dell");
        var acer = new Brand("Acer");
        var msi = new Brand("MSI");
        var jbl = new Brand("JBL");
        var sennheiser = new Brand("Sennheiser");
        var google = new Brand("Google");
        var lg = new Brand("LG");

        db.Brands.AddRange(apple, samsung, sony, xiaomi, asus, lenovo, hp, dell, acer, msi, jbl, sennheiser, google, lg);
        db.SaveChanges();

        // ─── Категории ───────────────────────────────
        var tvs = new Category("Телевизоры", "ТВ и мониторы", "/images/homepage/1.jpg");
        var laptops = new Category("Ноутбуки", "Все ноутбуки", "/images/homepage/2.webp");
        var computers = new Category("Компьютеры", "ПК и комплектующие", "/images/homepage/3.jpg");
        var phones = new Category("Смартфоны", "Смартфоны", "/images/homepage/4.png");
        var speakers = new Category("Колонки", "Аудио", "/images/homepage/5.webp");

        db.Categories.AddRange(laptops, computers, phones, tvs, speakers);
        db.SaveChanges();

        // ─── Типы ────────────────────────────────────
        var laptop = new ProductType("Ноутбук");
        var pc = new ProductType("ПК");
        var phone = new ProductType("Смартфон");
        var tv = new ProductType("Телевизор");
        var speaker = new ProductType("Колонка");

        db.Types.AddRange(laptop, pc, phone, tv, speaker);
        db.SaveChanges();

        // ─── Атрибуты (справочник) ───────────────────
        var attrCpu = new ProductAttribute("Процессор", "Основные", "");
        var attrRam = new ProductAttribute("ОЗУ", "Основные", "ГБ");
        var attrStorage = new ProductAttribute("Накопитель", "Основные", "ГБ");
        var attrGpu = new ProductAttribute("Видеокарта", "Основные", "");
        var attrScreen = new ProductAttribute("Экран", "Дисплей", "");
        var attrScreenSize = new ProductAttribute("Диагональ", "Дисплей", "дюйм");
        var attrResolution = new ProductAttribute("Разрешение", "Дисплей", "");
        var attrRefreshRate = new ProductAttribute("Частота обновления", "Дисплей", "Гц");
        var attrOs = new ProductAttribute("ОС", "Программное обеспечение", "");
        var attrSmartTv = new ProductAttribute("Smart TV", "Функции", "");
        var attrCamera = new ProductAttribute("Камера", "Камеры", "МП");
        var attrBattery = new ProductAttribute("Батарея", "Питание", "мАч");
        var attrPower = new ProductAttribute("Мощность", "Звук", "Вт");
        var attrBatteryLife = new ProductAttribute("Время работы", "Питание", "ч");
        var attrProtection = new ProductAttribute("Защита", "Функции", "");
        var attrBluetooth = new ProductAttribute("Bluetooth", "Связь", "");
        var attrWeight = new ProductAttribute("Вес", "Габариты", "кг");

        db.ProductAttributes.AddRange(
            attrCpu, attrRam, attrStorage, attrGpu,
            attrScreen, attrScreenSize, attrResolution, attrRefreshRate,
            attrOs, attrSmartTv, attrCamera, attrBattery,
            attrPower, attrBatteryLife, attrProtection, attrBluetooth, attrWeight);
        db.SaveChanges();

        var products = new List<Product>();

        // ═══════════════════════════════════════════
        // НОУТБУКИ (10 шт)
        // ═══════════════════════════════════════════
        var p1 = new Product(ProductName.Create("Apple MacBook Air M2"), Money.Rub(119999),
            apple.Id, laptops.Id, laptop.Id, "Ноутбук Apple MacBook Air M2");
        p1.AddImage("/images/laptop/Apple MacBook Air M2.webp", "MacBook Air M2", isMain: true);
        p1.AddAttributeValue(attrCpu.Id, "Apple M2");
        p1.AddAttributeValue(attrRam.Id, "8");
        p1.AddAttributeValue(attrStorage.Id, "256");
        p1.AddAttributeValue(attrScreen.Id, "Liquid Retina");
        p1.AddAttributeValue(attrScreenSize.Id, "13.6");
        p1.AddAttributeValue(attrOs.Id, "macOS");
        p1.AddAttributeValue(attrWeight.Id, "1.24");
        products.Add(p1);

        var p2 = new Product(ProductName.Create("ASUS VivoBook 15"), Money.Rub(54999),
            asus.Id, laptops.Id, laptop.Id, "Ноутбук ASUS VivoBook 15");
        p2.AddImage("/images/laptop/ASUS VivoBook 15.webp", "ASUS VivoBook 15", isMain: true);
        p2.AddAttributeValue(attrCpu.Id, "Intel Core i3-1215U");
        p2.AddAttributeValue(attrRam.Id, "8");
        p2.AddAttributeValue(attrStorage.Id, "256");
        p2.AddAttributeValue(attrScreen.Id, "Full HD IPS");
        p2.AddAttributeValue(attrScreenSize.Id, "15.6");
        p2.AddAttributeValue(attrOs.Id, "Windows 11");
        p2.AddAttributeValue(attrWeight.Id, "1.7");
        products.Add(p2);

        var p3 = new Product(ProductName.Create("Lenovo IdeaPad 3"), Money.Rub(44999),
            lenovo.Id, laptops.Id, laptop.Id, "Ноутбук Lenovo IdeaPad 3");
        p3.AddImage("/images/laptop/Lenovo IdeaPad 3.webp", "Lenovo IdeaPad 3", isMain: true);
        p3.AddAttributeValue(attrCpu.Id, "AMD Ryzen 5 5500U");
        p3.AddAttributeValue(attrRam.Id, "8");
        p3.AddAttributeValue(attrStorage.Id, "256");
        p3.AddAttributeValue(attrScreen.Id, "Full HD IPS");
        p3.AddAttributeValue(attrScreenSize.Id, "15.6");
        p3.AddAttributeValue(attrOs.Id, "Windows 11");
        p3.AddAttributeValue(attrWeight.Id, "1.65");
        products.Add(p3);

        var p4 = new Product(ProductName.Create("Xiaomi RedmiBook 15"), Money.Rub(39999),
            xiaomi.Id, laptops.Id, laptop.Id, "Ноутбук Xiaomi RedmiBook 15");
        p4.AddImage("/images/laptop/Xiaomi RedmiBook 15.webp", "RedmiBook 15", isMain: true);
        p4.AddAttributeValue(attrCpu.Id, "Intel Core i3-1115G4");
        p4.AddAttributeValue(attrRam.Id, "8");
        p4.AddAttributeValue(attrStorage.Id, "256");
        p4.AddAttributeValue(attrScreen.Id, "Full HD");
        p4.AddAttributeValue(attrScreenSize.Id, "15.6");
        p4.AddAttributeValue(attrOs.Id, "Windows 11");
        p4.AddAttributeValue(attrWeight.Id, "1.8");
        products.Add(p4);

        var p5 = new Product(ProductName.Create("ASUS TUF Gaming"), Money.Rub(79999),
            asus.Id, laptops.Id, laptop.Id, "Игровой ноутбук ASUS TUF");
        p5.AddImage("/images/laptop/ASUS TUF Gaming.webp", "ASUS TUF Gaming", isMain: true);
        p5.AddAttributeValue(attrCpu.Id, "Intel Core i5-11400H");
        p5.AddAttributeValue(attrRam.Id, "16");
        p5.AddAttributeValue(attrStorage.Id, "512");
        p5.AddAttributeValue(attrGpu.Id, "RTX 3050");
        p5.AddAttributeValue(attrScreen.Id, "Full HD 144Hz");
        p5.AddAttributeValue(attrScreenSize.Id, "15.6");
        p5.AddAttributeValue(attrOs.Id, "Windows 11");
        p5.AddAttributeValue(attrWeight.Id, "2.3");
        products.Add(p5);

        var p6 = new Product(ProductName.Create("ASUS ROG Strix G16"), Money.Rub(129999),
            asus.Id, laptops.Id, laptop.Id, "Игровой ноутбук, RTX 4060, i7-13650HX, 16GB DDR5, 512GB SSD");
        p6.AddImage("/images/laptop/ASUS ROG Strix G16.webp", "ASUS ROG Strix G16", isMain: true);
        p6.AddAttributeValue(attrCpu.Id, "Intel Core i7-13650HX");
        p6.AddAttributeValue(attrRam.Id, "16");
        p6.AddAttributeValue(attrStorage.Id, "512");
        p6.AddAttributeValue(attrGpu.Id, "RTX 4060");
        p6.AddAttributeValue(attrScreen.Id, "WQXGA 240Hz");
        p6.AddAttributeValue(attrScreenSize.Id, "16");
        p6.AddAttributeValue(attrOs.Id, "Windows 11");
        p6.AddAttributeValue(attrWeight.Id, "2.5");
        products.Add(p6);

        var p7 = new Product(ProductName.Create("Lenovo IdeaPad 5 Pro"), Money.Rub(89999),
            lenovo.Id, laptops.Id, laptop.Id, "Ультрабук, Ryzen 7 7840HS, 16GB, 1TB SSD, 16\" 2.5K");
        p7.AddImage("/images/laptop/Lenovo IdeaPad 5 Pro.webp", "IdeaPad 5 Pro", isMain: true);
        p7.AddAttributeValue(attrCpu.Id, "AMD Ryzen 7 7840HS");
        p7.AddAttributeValue(attrRam.Id, "16");
        p7.AddAttributeValue(attrStorage.Id, "1024");
        p7.AddAttributeValue(attrScreen.Id, "2.5K IPS");
        p7.AddAttributeValue(attrScreenSize.Id, "16");
        p7.AddAttributeValue(attrOs.Id, "Windows 11");
        p7.AddAttributeValue(attrWeight.Id, "2.0");
        products.Add(p7);

        var p8 = new Product(ProductName.Create("Apple MacBook Air M3"), Money.Rub(149999),
            apple.Id, laptops.Id, laptop.Id, "13.6\" Liquid Retina, M3, 8GB, 256GB SSD, macOS");
        p8.AddImage("/images/laptop/Apple MacBook Air M3.webp", "MacBook Air M3", isMain: true);
        p8.AddAttributeValue(attrCpu.Id, "Apple M3");
        p8.AddAttributeValue(attrRam.Id, "8");
        p8.AddAttributeValue(attrStorage.Id, "256");
        p8.AddAttributeValue(attrScreen.Id, "Liquid Retina");
        p8.AddAttributeValue(attrScreenSize.Id, "13.6");
        p8.AddAttributeValue(attrOs.Id, "macOS");
        p8.AddAttributeValue(attrWeight.Id, "1.24");
        products.Add(p8);

        var p9 = new Product(ProductName.Create("Xiaomi RedmiBook Pro 15"), Money.Rub(74999),
            xiaomi.Id, laptops.Id, laptop.Id, "Ryzen 7 6800H, 16GB, 512GB SSD, 15.6\" 3.2K 90Hz");
        p9.AddImage("/images/laptop/Xiaomi RedmiBook Pro 15.webp", "RedmiBook Pro 15", isMain: true);
        p9.AddAttributeValue(attrCpu.Id, "AMD Ryzen 7 6800H");
        p9.AddAttributeValue(attrRam.Id, "16");
        p9.AddAttributeValue(attrStorage.Id, "512");
        p9.AddAttributeValue(attrScreen.Id, "3.2K 90Hz");
        p9.AddAttributeValue(attrScreenSize.Id, "15.6");
        p9.AddAttributeValue(attrOs.Id, "Windows 11");
        p9.AddAttributeValue(attrWeight.Id, "1.8");
        products.Add(p9);

        var p10 = new Product(ProductName.Create("HP Victus 16"), Money.Rub(94999),
            hp.Id, laptops.Id, laptop.Id, "Игровой ноутбук, RTX 3050, i5-12500H, 16GB, 512GB SSD");
        p10.AddImage("/images/laptop/HP Victus 16.webp", "HP Victus 16", isMain: true);
        p10.AddAttributeValue(attrCpu.Id, "Intel Core i5-12500H");
        p10.AddAttributeValue(attrRam.Id, "16");
        p10.AddAttributeValue(attrStorage.Id, "512");
        p10.AddAttributeValue(attrGpu.Id, "RTX 3050");
        p10.AddAttributeValue(attrScreen.Id, "Full HD 144Hz");
        p10.AddAttributeValue(attrScreenSize.Id, "16.1");
        p10.AddAttributeValue(attrOs.Id, "Windows 11");
        p10.AddAttributeValue(attrWeight.Id, "2.5");
        products.Add(p10);

        // ═══════════════════════════════════════════
        // КОМПЬЮТЕРЫ (10 шт)
        // ═══════════════════════════════════════════
        var p11 = new Product(ProductName.Create("Игровой компьютер MSI"), Money.Rub(89999),
            msi.Id, computers.Id, pc.Id, "Игровой компьютер RGB");
        p11.AddImage("/images/computer/MSI Gaming PC.webp", "MSI Gaming", isMain: true);
        p11.AddAttributeValue(attrCpu.Id, "Intel Core i5-12400F");
        p11.AddAttributeValue(attrRam.Id, "16");
        p11.AddAttributeValue(attrStorage.Id, "512");
        p11.AddAttributeValue(attrGpu.Id, "RTX 3060");
        p11.AddAttributeValue(attrOs.Id, "Windows 11");
        products.Add(p11);

        var p12 = new Product(ProductName.Create("Офисный компьютер HP"), Money.Rub(44999),
            hp.Id, computers.Id, pc.Id, "Офисный компьютер");
        p12.AddImage("/images/computer/HP Office PC.webp", "HP Office", isMain: true);
        p12.AddAttributeValue(attrCpu.Id, "Intel Core i3-12100");
        p12.AddAttributeValue(attrRam.Id, "8");
        p12.AddAttributeValue(attrStorage.Id, "256");
        p12.AddAttributeValue(attrGpu.Id, "Встроенная");
        p12.AddAttributeValue(attrOs.Id, "Windows 11 Pro");
        products.Add(p12);

        var p13 = new Product(ProductName.Create("Домашний компьютер Dell"), Money.Rub(54999),
            dell.Id, computers.Id, pc.Id, "Домашний компьютер");
        p13.AddImage("/images/computer/Dell Home PC.jpg", "Dell Home", isMain: true);
        p13.AddAttributeValue(attrCpu.Id, "Intel Core i5-12400");
        p13.AddAttributeValue(attrRam.Id, "16");
        p13.AddAttributeValue(attrStorage.Id, "512");
        p13.AddAttributeValue(attrGpu.Id, "Встроенная");
        p13.AddAttributeValue(attrOs.Id, "Windows 11");
        products.Add(p13);

        var p14 = new Product(ProductName.Create("Игровой компьютер ASUS"), Money.Rub(109999),
            asus.Id, computers.Id, pc.Id, "Компьютер ASUS ROG");
        p14.AddImage("/images/computer/ASUS Gaming PC.webp", "ASUS ROG PC", isMain: true);
        p14.AddAttributeValue(attrCpu.Id, "AMD Ryzen 5 5600X");
        p14.AddAttributeValue(attrRam.Id, "16");
        p14.AddAttributeValue(attrStorage.Id, "512");
        p14.AddAttributeValue(attrGpu.Id, "RTX 3060 Ti");
        p14.AddAttributeValue(attrOs.Id, "Windows 11");
        products.Add(p14);

        var p15 = new Product(ProductName.Create("Бюджетный компьютер Lenovo"), Money.Rub(34999),
            lenovo.Id, computers.Id, pc.Id, "Бюджетный компьютер");
        p15.AddImage("/images/computer/Lenovo PC.webp", "Lenovo Budget", isMain: true);
        p15.AddAttributeValue(attrCpu.Id, "Intel Core i3-10100");
        p15.AddAttributeValue(attrRam.Id, "8");
        p15.AddAttributeValue(attrStorage.Id, "256");
        p15.AddAttributeValue(attrGpu.Id, "Встроенная");
        p15.AddAttributeValue(attrOs.Id, "Windows 10");
        products.Add(p15);

        var p16 = new Product(ProductName.Create("Игровой ПК MSI Infinite RS"), Money.Rub(189999),
            msi.Id, computers.Id, pc.Id, "RTX 4070 Ti, i7-13700KF, 32GB DDR5, 1TB NVMe");
        p16.AddImage("/images/computer/MSI Infinite RS.webp", "MSI Infinite RS", isMain: true);
        p16.AddAttributeValue(attrCpu.Id, "Intel Core i7-13700KF");
        p16.AddAttributeValue(attrRam.Id, "32");
        p16.AddAttributeValue(attrStorage.Id, "1024");
        p16.AddAttributeValue(attrGpu.Id, "RTX 4070 Ti");
        p16.AddAttributeValue(attrOs.Id, "Windows 11");
        products.Add(p16);

        var p17 = new Product(ProductName.Create("Офисный ПК HP ProDesk 400"), Money.Rub(44999),
            hp.Id, computers.Id, pc.Id, "i5-12500, 16GB DDR4, 512GB SSD, Windows 11 Pro");
        p17.AddImage("/images/computer/HP ProDesk 400.webp", "HP ProDesk 400", isMain: true);
        p17.AddAttributeValue(attrCpu.Id, "Intel Core i5-12500");
        p17.AddAttributeValue(attrRam.Id, "16");
        p17.AddAttributeValue(attrStorage.Id, "512");
        p17.AddAttributeValue(attrGpu.Id, "Встроенная");
        p17.AddAttributeValue(attrOs.Id, "Windows 11 Pro");
        products.Add(p17);

        var p18 = new Product(ProductName.Create("Домашний ПК Dell XPS 8960"), Money.Rub(99999),
            dell.Id, computers.Id, pc.Id, "RTX 3060, i7-13700, 16GB, 1TB SSD");
        p18.AddImage("/images/computer/Dell XPS 8960.webp", "Dell XPS 8960", isMain: true);
        p18.AddAttributeValue(attrCpu.Id, "Intel Core i7-13700");
        p18.AddAttributeValue(attrRam.Id, "16");
        p18.AddAttributeValue(attrStorage.Id, "1024");
        p18.AddAttributeValue(attrGpu.Id, "RTX 3060");
        p18.AddAttributeValue(attrOs.Id, "Windows 11");
        products.Add(p18);

        var p19 = new Product(ProductName.Create("ASUS ROG Strix GT35"), Money.Rub(159999),
            asus.Id, computers.Id, pc.Id, "RTX 4070, Ryzen 7 7700X, 32GB DDR5, 1TB SSD");
        p19.AddImage("/images/computer/ASUS ROG Strix GT35.webp", "ASUS ROG GT35", isMain: true);
        p19.AddAttributeValue(attrCpu.Id, "AMD Ryzen 7 7700X");
        p19.AddAttributeValue(attrRam.Id, "32");
        p19.AddAttributeValue(attrStorage.Id, "1024");
        p19.AddAttributeValue(attrGpu.Id, "RTX 4070");
        p19.AddAttributeValue(attrOs.Id, "Windows 11");
        products.Add(p19);

        var p20 = new Product(ProductName.Create("Бюджетный ПК Acer Aspire TC"), Money.Rub(34999),
            acer.Id, computers.Id, pc.Id, "i3-12100, 8GB, 256GB SSD, без видеокарты");
        p20.AddImage("/images/computer/Acer Aspire TC.jpg", "Acer Aspire TC", isMain: true);
        p20.AddAttributeValue(attrCpu.Id, "Intel Core i3-12100");
        p20.AddAttributeValue(attrRam.Id, "8");
        p20.AddAttributeValue(attrStorage.Id, "256");
        p20.AddAttributeValue(attrGpu.Id, "Встроенная");
        p20.AddAttributeValue(attrOs.Id, "Windows 11");
        products.Add(p20);

        // ═══════════════════════════════════════════
        // ТЕЛЕВИЗОРЫ (10 шт)
        // ═══════════════════════════════════════════
        var p21 = new Product(ProductName.Create("Samsung QE55Q80AAUXCE"), Money.Rub(79999),
            samsung.Id, tvs.Id, tv.Id, "Телевизор Samsung QLED 4K");
        p21.AddImage("/images/products/Samsung QE55Q80AAUXCE.webp", "Samsung QLED", isMain: true);
        p21.AddAttributeValue(attrScreenSize.Id, "55");
        p21.AddAttributeValue(attrResolution.Id, "4K UHD");
        p21.AddAttributeValue(attrScreen.Id, "QLED");
        p21.AddAttributeValue(attrRefreshRate.Id, "120");
        p21.AddAttributeValue(attrSmartTv.Id, "Tizen");
        products.Add(p21);

        var p22 = new Product(ProductName.Create("LG OLED65C24LA"), Money.Rub(149999),
            lg.Id, tvs.Id, tv.Id, "Телевизор LG OLED 4K");
        p22.AddImage("/images/products/LG OLED65C24LA.jpg", "LG OLED", isMain: true);
        p22.AddAttributeValue(attrScreenSize.Id, "65");
        p22.AddAttributeValue(attrResolution.Id, "4K UHD");
        p22.AddAttributeValue(attrScreen.Id, "OLED evo");
        p22.AddAttributeValue(attrRefreshRate.Id, "120");
        p22.AddAttributeValue(attrSmartTv.Id, "webOS");
        products.Add(p22);

        var p23 = new Product(ProductName.Create("Sony KD-43X80K"), Money.Rub(54999),
            sony.Id, tvs.Id, tv.Id, "Телевизор Sony BRAVIA");
        p23.AddImage("/images/products/Sony KD-43X80K.png", "Sony BRAVIA", isMain: true);
        p23.AddAttributeValue(attrScreenSize.Id, "43");
        p23.AddAttributeValue(attrResolution.Id, "4K UHD");
        p23.AddAttributeValue(attrScreen.Id, "LED");
        p23.AddAttributeValue(attrRefreshRate.Id, "60");
        p23.AddAttributeValue(attrSmartTv.Id, "Google TV");
        products.Add(p23);

        var p24 = new Product(ProductName.Create("Xiaomi Mi TV P1 50"), Money.Rub(39999),
            xiaomi.Id, tvs.Id, tv.Id, "Телевизор Xiaomi 4K");
        p24.AddImage("/images/products/Xiaomi Mi TV P1 50.jpg", "Xiaomi TV P1", isMain: true);
        p24.AddAttributeValue(attrScreenSize.Id, "50");
        p24.AddAttributeValue(attrResolution.Id, "4K UHD");
        p24.AddAttributeValue(attrScreen.Id, "LED");
        p24.AddAttributeValue(attrRefreshRate.Id, "60");
        p24.AddAttributeValue(attrSmartTv.Id, "Android TV");
        products.Add(p24);

        var p25 = new Product(ProductName.Create("Samsung UE32T5300AUXCE"), Money.Rub(29999),
            samsung.Id, tvs.Id, tv.Id, "Телевизор Samsung HD");
        p25.AddImage("/images/products/Samsung UE32T5300AUXCE.webp", "Samsung UE32", isMain: true);
        p25.AddAttributeValue(attrScreenSize.Id, "32");
        p25.AddAttributeValue(attrResolution.Id, "HD");
        p25.AddAttributeValue(attrScreen.Id, "LED");
        p25.AddAttributeValue(attrRefreshRate.Id, "60");
        p25.AddAttributeValue(attrSmartTv.Id, "Tizen");
        products.Add(p25);

        var p46 = new Product(ProductName.Create("Samsung QE55Q80C"), Money.Rub(109999),
            samsung.Id, tvs.Id, tv.Id, "55\" QLED 4K, 120Hz, Smart TV Tizen");
        p46.AddImage("/images/products/Samsung QE55Q80C.jpg", "Samsung Q80C", isMain: true);
        p46.AddAttributeValue(attrScreenSize.Id, "55");
        p46.AddAttributeValue(attrResolution.Id, "4K UHD");
        p46.AddAttributeValue(attrScreen.Id, "QLED");
        p46.AddAttributeValue(attrRefreshRate.Id, "120");
        p46.AddAttributeValue(attrSmartTv.Id, "Tizen");
        products.Add(p46);

        var p47 = new Product(ProductName.Create("LG OLED55C3"), Money.Rub(149999),
            lg.Id, tvs.Id, tv.Id, "55\" OLED evo 4K, 120Hz, webOS, Dolby Vision");
        p47.AddImage("/images/products/LG OLED55C3.webp", "LG OLED C3", isMain: true);
        p47.AddAttributeValue(attrScreenSize.Id, "55");
        p47.AddAttributeValue(attrResolution.Id, "4K UHD");
        p47.AddAttributeValue(attrScreen.Id, "OLED evo");
        p47.AddAttributeValue(attrRefreshRate.Id, "120");
        p47.AddAttributeValue(attrSmartTv.Id, "webOS");
        products.Add(p47);

        var p48 = new Product(ProductName.Create("Sony XR-55A80L"), Money.Rub(179999),
            sony.Id, tvs.Id, tv.Id, "55\" OLED 4K, Cognitive Processor XR, Google TV");
        p48.AddImage("/images/products/Sony XR-55A80L.webp", "Sony XR A80L", isMain: true);
        p48.AddAttributeValue(attrScreenSize.Id, "55");
        p48.AddAttributeValue(attrResolution.Id, "4K UHD");
        p48.AddAttributeValue(attrScreen.Id, "OLED");
        p48.AddAttributeValue(attrRefreshRate.Id, "120");
        p48.AddAttributeValue(attrSmartTv.Id, "Google TV");
        products.Add(p48);

        var p49 = new Product(ProductName.Create("Xiaomi TV Q2 55"), Money.Rub(59999),
            xiaomi.Id, tvs.Id, tv.Id, "55\" QLED 4K, Android TV, Dolby Vision");
        p49.AddImage("/images/products/Xiaomi TV Q2 55.webp", "Xiaomi TV Q2", isMain: true);
        p49.AddAttributeValue(attrScreenSize.Id, "55");
        p49.AddAttributeValue(attrResolution.Id, "4K UHD");
        p49.AddAttributeValue(attrScreen.Id, "QLED");
        p49.AddAttributeValue(attrRefreshRate.Id, "60");
        p49.AddAttributeValue(attrSmartTv.Id, "Android TV");
        products.Add(p49);

        var p50 = new Product(ProductName.Create("Samsung UE65AU7100"), Money.Rub(74999),
            samsung.Id, tvs.Id, tv.Id, "65\" LED 4K, Crystal Processor, Smart TV");
        p50.AddImage("/images/products/Samsung UE65AU7100.webp", "Samsung UE65", isMain: true);
        p50.AddAttributeValue(attrScreenSize.Id, "65");
        p50.AddAttributeValue(attrResolution.Id, "4K UHD");
        p50.AddAttributeValue(attrScreen.Id, "LED");
        p50.AddAttributeValue(attrRefreshRate.Id, "60");
        p50.AddAttributeValue(attrSmartTv.Id, "Tizen");
        products.Add(p50);

        // ═══════════════════════════════════════════
        // СМАРТФОНЫ (10 шт)
        // ═══════════════════════════════════════════
        var p26 = new Product(ProductName.Create("Apple iPhone 15 Pro"), Money.Rub(119999),
            apple.Id, phones.Id, phone.Id, "Смартфон Apple iPhone 15 Pro");
        p26.AddImage("/images/phone/Apple iPhone 15 Pro.webp", "iPhone 15 Pro", isMain: true);
        p26.AddAttributeValue(attrScreen.Id, "Super Retina XDR");
        p26.AddAttributeValue(attrScreenSize.Id, "6.1");
        p26.AddAttributeValue(attrCpu.Id, "Apple A17 Pro");
        p26.AddAttributeValue(attrRam.Id, "8");
        p26.AddAttributeValue(attrStorage.Id, "128");
        p26.AddAttributeValue(attrCamera.Id, "48");
        p26.AddAttributeValue(attrBattery.Id, "3274");
        p26.AddAttributeValue(attrOs.Id, "iOS 17");
        products.Add(p26);

        var p27 = new Product(ProductName.Create("Samsung Galaxy S24"), Money.Rub(89999),
            samsung.Id, phones.Id, phone.Id, "Смартфон Samsung Galaxy S24");
        p27.AddImage("/images/phone/Samsung Galaxy S24.webp", "Galaxy S24", isMain: true);
        p27.AddAttributeValue(attrScreen.Id, "Dynamic AMOLED 2X 120Hz");
        p27.AddAttributeValue(attrScreenSize.Id, "6.2");
        p27.AddAttributeValue(attrCpu.Id, "Snapdragon 8 Gen 3");
        p27.AddAttributeValue(attrRam.Id, "8");
        p27.AddAttributeValue(attrStorage.Id, "256");
        p27.AddAttributeValue(attrCamera.Id, "50");
        p27.AddAttributeValue(attrBattery.Id, "4000");
        p27.AddAttributeValue(attrOs.Id, "Android 14");
        products.Add(p27);

        var p28 = new Product(ProductName.Create("Xiaomi 14 Ultra"), Money.Rub(69999),
            xiaomi.Id, phones.Id, phone.Id, "Смартфон Xiaomi 14 Ultra");
        p28.AddImage("/images/phone/Xiaomi 14 Ultra.webp", "Xiaomi 14 Ultra", isMain: true);
        p28.AddAttributeValue(attrScreen.Id, "AMOLED 120Hz");
        p28.AddAttributeValue(attrScreenSize.Id, "6.73");
        p28.AddAttributeValue(attrCpu.Id, "Snapdragon 8 Gen 3");
        p28.AddAttributeValue(attrRam.Id, "16");
        p28.AddAttributeValue(attrStorage.Id, "512");
        p28.AddAttributeValue(attrCamera.Id, "50");
        p28.AddAttributeValue(attrBattery.Id, "5300");
        p28.AddAttributeValue(attrOs.Id, "Android 14");
        products.Add(p28);

        var p29 = new Product(ProductName.Create("Google Pixel 8"), Money.Rub(64999),
            google.Id, phones.Id, phone.Id, "Смартфон Google Pixel 8");
        p29.AddImage("/images/phone/Google Pixel 8.webp", "Pixel 8", isMain: true);
        p29.AddAttributeValue(attrScreen.Id, "OLED 120Hz");
        p29.AddAttributeValue(attrScreenSize.Id, "6.2");
        p29.AddAttributeValue(attrCpu.Id, "Google Tensor G3");
        p29.AddAttributeValue(attrRam.Id, "8");
        p29.AddAttributeValue(attrStorage.Id, "128");
        p29.AddAttributeValue(attrCamera.Id, "50");
        p29.AddAttributeValue(attrBattery.Id, "4575");
        p29.AddAttributeValue(attrOs.Id, "Android 14");
        products.Add(p29);

        var p30 = new Product(ProductName.Create("Sony Xperia 1 V"), Money.Rub(79999),
            sony.Id, phones.Id, phone.Id, "Смартфон Sony Xperia 1 V");
        p30.AddImage("/images/phone/Sony Xperia 1 V.jpg", "Xperia 1 V", isMain: true);
        p30.AddAttributeValue(attrScreen.Id, "4K OLED 120Hz");
        p30.AddAttributeValue(attrScreenSize.Id, "6.5");
        p30.AddAttributeValue(attrCpu.Id, "Snapdragon 8 Gen 2");
        p30.AddAttributeValue(attrRam.Id, "12");
        p30.AddAttributeValue(attrStorage.Id, "256");
        p30.AddAttributeValue(attrCamera.Id, "52");
        p30.AddAttributeValue(attrBattery.Id, "5000");
        p30.AddAttributeValue(attrOs.Id, "Android 13");
        products.Add(p30);

        var p36 = new Product(ProductName.Create("Apple iPhone 15 Pro Max"), Money.Rub(139999),
            apple.Id, phones.Id, phone.Id, "6.7\" Super Retina XDR, A17 Pro, 256GB, титан");
        p36.AddImage("/images/phone/Apple iPhone 15 Pro Max.webp", "iPhone 15 Pro Max", isMain: true);
        p36.AddAttributeValue(attrScreen.Id, "Super Retina XDR 120Hz");
        p36.AddAttributeValue(attrScreenSize.Id, "6.7");
        p36.AddAttributeValue(attrCpu.Id, "Apple A17 Pro");
        p36.AddAttributeValue(attrRam.Id, "8");
        p36.AddAttributeValue(attrStorage.Id, "256");
        p36.AddAttributeValue(attrCamera.Id, "48");
        p36.AddAttributeValue(attrBattery.Id, "4441");
        p36.AddAttributeValue(attrOs.Id, "iOS 17");
        products.Add(p36);

        var p37 = new Product(ProductName.Create("Samsung Galaxy S24 Ultra"), Money.Rub(129999),
            samsung.Id, phones.Id, phone.Id, "6.8\" QHD+, Snapdragon 8 Gen 3, 12GB, 256GB, S-Pen");
        p37.AddImage("/images/phone/Samsung Galaxy S24 Ultra.jpg", "Galaxy S24 Ultra", isMain: true);
        p37.AddAttributeValue(attrScreen.Id, "Dynamic AMOLED 2X 120Hz");
        p37.AddAttributeValue(attrScreenSize.Id, "6.8");
        p37.AddAttributeValue(attrCpu.Id, "Snapdragon 8 Gen 3");
        p37.AddAttributeValue(attrRam.Id, "12");
        p37.AddAttributeValue(attrStorage.Id, "256");
        p37.AddAttributeValue(attrCamera.Id, "200");
        p37.AddAttributeValue(attrBattery.Id, "5000");
        p37.AddAttributeValue(attrOs.Id, "Android 14");
        products.Add(p37);

        var p38 = new Product(ProductName.Create("Xiaomi 14 Pro"), Money.Rub(89999),
            xiaomi.Id, phones.Id, phone.Id, "6.73\" AMOLED, Snapdragon 8 Gen 3, 12GB, 512GB");
        p38.AddImage("/images/phone/Xiaomi 14 Pro.webp", "Xiaomi 14 Pro", isMain: true);
        p38.AddAttributeValue(attrScreen.Id, "AMOLED 120Hz");
        p38.AddAttributeValue(attrScreenSize.Id, "6.73");
        p38.AddAttributeValue(attrCpu.Id, "Snapdragon 8 Gen 3");
        p38.AddAttributeValue(attrRam.Id, "12");
        p38.AddAttributeValue(attrStorage.Id, "512");
        p38.AddAttributeValue(attrCamera.Id, "50");
        p38.AddAttributeValue(attrBattery.Id, "4880");
        p38.AddAttributeValue(attrOs.Id, "Android 14");
        products.Add(p38);

        var p39 = new Product(ProductName.Create("Google Pixel 8 Pro"), Money.Rub(99999),
            google.Id, phones.Id, phone.Id, "6.7\" LTPO OLED, Tensor G3, 12GB, 128GB");
        p39.AddImage("/images/phone/Google Pixel 8 Pro.jpg", "Pixel 8 Pro", isMain: true);
        p39.AddAttributeValue(attrScreen.Id, "LTPO OLED 120Hz");
        p39.AddAttributeValue(attrScreenSize.Id, "6.7");
        p39.AddAttributeValue(attrCpu.Id, "Google Tensor G3");
        p39.AddAttributeValue(attrRam.Id, "12");
        p39.AddAttributeValue(attrStorage.Id, "128");
        p39.AddAttributeValue(attrCamera.Id, "50");
        p39.AddAttributeValue(attrBattery.Id, "5050");
        p39.AddAttributeValue(attrOs.Id, "Android 14");
        products.Add(p39);

        var p40 = new Product(ProductName.Create("Sony Xperia 1 V"), Money.Rub(109999),
            sony.Id, phones.Id, phone.Id, "6.5\" 4K OLED, Snapdragon 8 Gen 2, 12GB, 256GB");
        p40.AddImage("/images/phone/Sony Xperia 1 Vblack.webp", "Xperia 1 V Black", isMain: true);
        p40.AddAttributeValue(attrScreen.Id, "4K OLED 120Hz");
        p40.AddAttributeValue(attrScreenSize.Id, "6.5");
        p40.AddAttributeValue(attrCpu.Id, "Snapdragon 8 Gen 2");
        p40.AddAttributeValue(attrRam.Id, "12");
        p40.AddAttributeValue(attrStorage.Id, "256");
        p40.AddAttributeValue(attrCamera.Id, "52");
        p40.AddAttributeValue(attrBattery.Id, "5000");
        p40.AddAttributeValue(attrOs.Id, "Android 13");
        products.Add(p40);

        // ═══════════════════════════════════════════
        // КОЛОНКИ (10 шт)
        // ═══════════════════════════════════════════
        var p31 = new Product(ProductName.Create("JBL Charge 5"), Money.Rub(12999),
            jbl.Id, speakers.Id, speaker.Id, "Колонка JBL Charge 5");
        p31.AddImage("/images/speaker/JBL Charge 5.webp", "JBL Charge 5", isMain: true);
        p31.AddAttributeValue(attrPower.Id, "30");
        p31.AddAttributeValue(attrBatteryLife.Id, "20");
        p31.AddAttributeValue(attrProtection.Id, "IP67");
        p31.AddAttributeValue(attrBluetooth.Id, "5.1");
        p31.AddAttributeValue(attrWeight.Id, "0.96");
        products.Add(p31);

        var p32 = new Product(ProductName.Create("JBL Flip 6"), Money.Rub(9999),
            jbl.Id, speakers.Id, speaker.Id, "Колонка JBL Flip 6");
        p32.AddImage("/images/speaker/JBL Flip 6.webp", "JBL Flip 6", isMain: true);
        p32.AddAttributeValue(attrPower.Id, "20");
        p32.AddAttributeValue(attrBatteryLife.Id, "12");
        p32.AddAttributeValue(attrProtection.Id, "IP67");
        p32.AddAttributeValue(attrBluetooth.Id, "5.1");
        p32.AddAttributeValue(attrWeight.Id, "0.55");
        products.Add(p32);

        var p33 = new Product(ProductName.Create("Sony SRS-XB43"), Money.Rub(14999),
            sony.Id, speakers.Id, speaker.Id, "Колонка Sony SRS-XB43");
        p33.AddImage("/images/speaker/Sony SRS-XB43.jpg", "Sony SRS-XB43", isMain: true);
        p33.AddAttributeValue(attrPower.Id, "30");
        p33.AddAttributeValue(attrBatteryLife.Id, "24");
        p33.AddAttributeValue(attrProtection.Id, "IP67");
        p33.AddAttributeValue(attrBluetooth.Id, "5.0");
        p33.AddAttributeValue(attrWeight.Id, "0.98");
        products.Add(p33);

        var p34 = new Product(ProductName.Create("Sennheiser AMBEO"), Money.Rub(29999),
            sennheiser.Id, speakers.Id, speaker.Id, "Колонка Sennheiser AMBEO");
        p34.AddImage("/images/speaker/Sennheiser AMBEO.webp", "Sennheiser AMBEO", isMain: true);
        p34.AddAttributeValue(attrPower.Id, "60");
        p34.AddAttributeValue(attrBluetooth.Id, "5.0");
        p34.AddAttributeValue(attrWeight.Id, "1.2");
        products.Add(p34);

        var p35 = new Product(ProductName.Create("Xiaomi Mi Portable"), Money.Rub(3999),
            xiaomi.Id, speakers.Id, speaker.Id, "Колонка Xiaomi Mi Portable");
        p35.AddImage("/images/speaker/Xiaomi Mi Portable.webp", "Xiaomi Mi Portable", isMain: true);
        p35.AddAttributeValue(attrPower.Id, "16");
        p35.AddAttributeValue(attrBatteryLife.Id, "13");
        p35.AddAttributeValue(attrProtection.Id, "IPX7");
        p35.AddAttributeValue(attrBluetooth.Id, "5.0");
        p35.AddAttributeValue(attrWeight.Id, "0.45");
        products.Add(p35);

        var p41 = new Product(ProductName.Create("JBL Charge 5"), Money.Rub(17999),
            jbl.Id, speakers.Id, speaker.Id, "Портативная колонка, 40Вт, IP67, 20ч работы");
        p41.AddImage("/images/speaker/JBL Charge 5white.webp", "JBL Charge 5 White", isMain: true);
        p41.AddAttributeValue(attrPower.Id, "40");
        p41.AddAttributeValue(attrBatteryLife.Id, "20");
        p41.AddAttributeValue(attrProtection.Id, "IP67");
        p41.AddAttributeValue(attrBluetooth.Id, "5.1");
        p41.AddAttributeValue(attrWeight.Id, "0.96");
        products.Add(p41);

        var p42 = new Product(ProductName.Create("JBL Flip 6"), Money.Rub(12999),
            jbl.Id, speakers.Id, speaker.Id, "Портативная колонка, 30Вт, IP67, 12ч работы");
        p42.AddImage("/images/speaker/JBL Flip6new.webp", "JBL Flip 6 New", isMain: true);
        p42.AddAttributeValue(attrPower.Id, "30");
        p42.AddAttributeValue(attrBatteryLife.Id, "12");
        p42.AddAttributeValue(attrProtection.Id, "IP67");
        p42.AddAttributeValue(attrBluetooth.Id, "5.1");
        p42.AddAttributeValue(attrWeight.Id, "0.55");
        products.Add(p42);

        var p43 = new Product(ProductName.Create("Sony SRS-XB43"), Money.Rub(19999),
            sony.Id, speakers.Id, speaker.Id, "Портативная колонка, Extra Bass, IP67, 24ч");
        p43.AddImage("/images/speaker/Sony SRS-XB43new.jpg", "Sony SRS-XB43 New", isMain: true);
        p43.AddAttributeValue(attrPower.Id, "30");
        p43.AddAttributeValue(attrBatteryLife.Id, "24");
        p43.AddAttributeValue(attrProtection.Id, "IP67");
        p43.AddAttributeValue(attrBluetooth.Id, "5.0");
        p43.AddAttributeValue(attrWeight.Id, "0.98");
        products.Add(p43);

        var p44 = new Product(ProductName.Create("Sennheiser Momentum 4"), Money.Rub(34999),
            sennheiser.Id, speakers.Id, speaker.Id, "Беспроводная колонка, Hi-Fi звук, 60Вт");
        p44.AddImage("/images/speaker/Sennheiser Momentum 4.webp", "Momentum 4", isMain: true);
        p44.AddAttributeValue(attrPower.Id, "60");
        p44.AddAttributeValue(attrBluetooth.Id, "5.2");
        p44.AddAttributeValue(attrWeight.Id, "0.8");
        products.Add(p44);

        var p45 = new Product(ProductName.Create("Xiaomi Mi Sound Pro"), Money.Rub(9999),
            xiaomi.Id, speakers.Id, speaker.Id, "Умная колонка, 30Вт, Bluetooth 5.0, голосовой помощник");
        p45.AddImage("/images/speaker/Xiaomi Mi Sound Pro.jpg", "Mi Sound Pro", isMain: true);
        p45.AddAttributeValue(attrPower.Id, "30");
        p45.AddAttributeValue(attrBluetooth.Id, "5.0");
        p45.AddAttributeValue(attrWeight.Id, "0.7");
        products.Add(p45);

        // Остатки на складе: детерминированно, у каждого седьмого товара — 0 (для демонстрации «нет в наличии»)
        for (var i = 0; i < products.Count; i++)
            products[i].SetStock(i % 7 == 6 ? 0 : 5 + (i * 3) % 20);

        db.Products.AddRange(products);
        db.SaveChanges();
    }
}