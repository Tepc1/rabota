import pandas as pd
import random
import os
from datetime import datetime, timedelta
import openpyxl

# Реальные продукты и материалы для разных отраслей
INDUSTRY_DATA = {
    "dairy": {
        "products": [
            "Сметана классическая 15% 540г",
            "Сметана классическая 20% 540г",
            "Кефир 2,5% 900г",
            "Кефир 3,2% 900г",
            "Молоко 2,5% 900г",
            "Молоко 3,2% 900г"
        ],
        "materials": {
            "Сметана классическая 15% 540г": [("Молоко нормализованное", 0.900), ("Закваска сметанная", 0.070)],
            "Сметана классическая 20% 540г": [("Молоко нормализованное", 0.880), ("Закваска сметанная", 0.075)],
            "Кефир 2,5% 900г": [("Молоко нормализованное", 0.920), ("Закваска кефирная", 0.050)],
            "Кефир 3,2% 900г": [("Молоко нормализованное", 0.910), ("Закваска кефирная", 0.055)],
            "Молоко 2,5% 900г": [("Молоко нормализованное", 0.950)],
            "Молоко 3,2% 900г": [("Молоко нормализованное", 0.940)]
        }
    },
    "bakery": {
        "products": [
            "Хлеб пшеничный 400г",
            "Хлеб ржаной 500г",
            "Батон нарезной 500г",
            "Булочка с маком 80г",
            "Багет французский 250г"
        ],
        "materials": {
            "Хлеб пшеничный 400г": [("Мука пшеничная в/с", 0.350), ("Дрожжи", 0.005), ("Соль", 0.006)],
            "Хлеб ржаной 500г": [("Мука ржаная", 0.400), ("Дрожжи", 0.006), ("Соль", 0.008)],
            "Батон нарезной 500г": [("Мука пшеничная в/с", 0.400), ("Сахар", 0.030), ("Маргарин", 0.020)],
            "Булочка с маком 80г": [("Мука пшеничная в/с", 0.060), ("Мак", 0.010), ("Сахар", 0.010)]
        }
    },
    "furniture": {
        "products": [
            "Стул офисный",
            "Стол письменный",
            "Шкаф 2-дверный",
            "Кровать двуспальная",
            "Комод 4-ящичный"
        ],
        "materials": {
            "Стул офисный": [("ЛДСП 16мм", 1.5), ("Кромка ПВХ", 4.0), ("Конфирматы", 0.020)],
            "Стол письменный": [("ЛДСП 16мм", 3.2), ("Кромка ПВХ", 8.5), ("Конфирматы", 0.040)],
            "Шкаф 2-дверный": [("ЛДСП 16мм", 8.5), ("Кромка ПВХ", 15.0), ("Петли", 4.0)]
        }
    },
    "sewing": {
        "products": [
            "Футболка хлопковая",
            "Рубашка мужская",
            "Брюки женские",
            "Платье летнее"
        ],
        "materials": {
            "Футболка хлопковая": [("Ткань хлопок", 0.250), ("Нитки", 0.050)],
            "Рубашка мужская": [("Ткань хлопок", 1.200), ("Пуговицы", 8.0), ("Нитки", 0.080)],
            "Брюки женские": [("Ткань полиэстер", 1.100), ("Молния", 1.0), ("Нитки", 0.070)]
        }
    },
    "confectionery": {
        "products": [
            "Торт Наполеон 1кг",
            "Пирожное картошка 100г",
            "Печенье овсяное 200г",
            "Кекс ванильный 150г"
        ],
        "materials": {
            "Торт Наполеон 1кг": [("Мука пшеничная", 0.400), ("Масло сливочное", 0.300), ("Молоко сгущенное", 0.380)],
            "Пирожное картошка 100г": [("Печенье", 0.070), ("Молоко сгущенное", 0.040), ("Какао", 0.010)],
            "Печенье овсяное 200г": [("Мука пшеничная", 0.120), ("Сахар", 0.060), ("Масло сливочное", 0.050)]
        }
    },
    "auto": {
        "products": [
            "Замена масла ДВС",
            "Замена тормозных колодок",
            "Замена свечей зажигания",
            "Диагностика подвески"
        ],
        "materials": {
            "Замена масла ДВС": [("Масло моторное 5W-40", 4.0), ("Фильтр масляный", 1.0)],
            "Замена тормозных колодок": [("Колодки тормозные", 1.0), ("Тормозная жидкость", 0.5)],
            "Замена свечей зажигания": [("Свечи зажигания", 4.0)]
        }
    },
    "construction": {
        "products": [
            "Бетон М200 (1м³)",
            "Бетон М300 (1м³)",
            "Раствор цементный (1м³)",
            "Кирпичная кладка (1м³)"
        ],
        "materials": {
            "Бетон М200 (1м³)": [("Цемент М500", 0.280), ("Песок речной", 0.750), ("Щебень 5-20", 1.100)],
            "Бетон М300 (1м³)": [("Цемент М500", 0.350), ("Песок речной", 0.700), ("Щебень 5-20", 1.050)],
            "Раствор цементный (1м³)": [("Цемент М500", 0.400), ("Песок речной", 1.200)]
        }
    },
    "chemical": {
        "products": [
            "Шампунь для волос 250мл",
            "Жидкое мыло 500мл",
            "Крем для рук 100мл",
            "Гель для душа 400мл"
        ],
        "materials": {
            "Шампунь для волос 250мл": [("Основа моющая", 0.200), ("Глицерин", 0.010), ("Отдушка", 0.005)],
            "Жидкое мыло 500мл": [("Основа моющая", 0.400), ("Глицерин", 0.020), ("Консервант", 0.002)],
            "Крем для рук 100мл": [("Глицерин", 0.030), ("Отдушка", 0.003), ("Консервант", 0.001)]
        }
    }
}

# Цены на материалы по отраслям
MATERIAL_PRICES = {
    "dairy": {"Молоко нормализованное": 34, "Закваска сметанная": 45, "Закваска кефирная": 12},
    "bakery": {"Мука пшеничная в/с": 45, "Мука ржаная": 40, "Дрожжи": 120, "Соль": 8, "Сахар": 60, "Маргарин": 140, "Мак": 300},
    "furniture": {"ЛДСП 16мм": 1200, "Кромка ПВХ": 45, "Конфирматы": 2, "Петли": 150},
    "sewing": {"Ткань хлопок": 350, "Ткань полиэстер": 280, "Нитки": 50, "Пуговицы": 5, "Молния": 80},
    "confectionery": {"Мука пшеничная": 45, "Масло сливочное": 450, "Молоко сгущенное": 120, "Печенье": 180, "Сахар": 60, "Какао": 250},
    "auto": {"Масло моторное 5W-40": 450, "Фильтр масляный": 350, "Колодки тормозные": 1200, "Тормозная жидкость": 280, "Свечи зажигания": 250},
    "construction": {"Цемент М500": 450, "Песок речной": 350, "Щебень 5-20": 650},
    "chemical": {"Основа моющая": 180, "Глицерин": 220, "Отдушка": 450, "Консервант": 380}
}

# Названия компаний
COMPANY_NAMES = [
    "Молочный_комбинат_Полесье", "Хлебозавод_№1", "Мебельная_фабрика_Уют",
    "Швейное_ателье_Мода", "Кондитерская_фабрика_Сладость", "Автомастерская_Сервис",
    "Бетонный_завод_Строй", "Косметическая_лаборатория_Бьюти", "Молокозавод_Фермерский",
    "Пекарня_Домашняя", "Цех_корпусной_мебели", "Трикотажная_фабрика",
    "Фабрика_десертов", "СТО_Профессионал", "ЖБИ_Комбинат",
    "Парфюмерный_цех", "Молочная_ферма", "Хлебпекарня_Свежий_хлеб",
    "Мебельный_дом", "Ателье_Индивидуальный_пошив", "Кондитерский_дом",
    "Автоцентр_Плюс", "Завод_товарного_бетона", "Лаборатория_косметики",
    "Молочный_двор", "Городская_пекарня", "Фабрика_мягкой_мебели",
    "Швейный_цех", "Сладкая_жизнь", "Техцентр_Авто"
]

def get_industry_type(company_name):
    """Определяет тип отрасли по названию компании"""
    name_lower = company_name.lower()
    if any(k in name_lower for k in ["молоч", "молоко", "ферм"]):
        return "dairy"
    elif any(k in name_lower for k in ["хлеб", "пекар", "булоч"]):
        return "bakery"
    elif any(k in name_lower for k in ["мебел", "шкаф", "диван", "цех"]):
        return "furniture"
    elif any(k in name_lower for k in ["швей", "ателье", "трикотаж", "одежд"]):
        return "sewing"
    elif any(k in name_lower for k in ["кондитер", "десерт", "сладк", "торт", "пекарня"]):
        return "confectionery"
    elif any(k in name_lower for k in ["авто", "сто", "техцентр", "сервис"]):
        return "auto"
    elif any(k in name_lower for k in ["бетон", "жби", "строй"]):
        return "construction"
    elif any(k in name_lower for k in ["космет", "парфюм", "бьюти"]):
        return "chemical"
    else:
        return random.choice(list(INDUSTRY_DATA.keys()))

def create_variant(var_id, company_name):
    folder = f"Variant_{var_id:02d}_{company_name}"
    os.makedirs(folder, exist_ok=True)
    
    # Получаем данные для отрасли
    industry_type = get_industry_type(company_name)
    industry_data = INDUSTRY_DATA[industry_type]
    material_prices = MATERIAL_PRICES.get(industry_type, {})
    
    # Выбираем 3 случайных продукта
    selected_products = random.sample(industry_data["products"], min(3, len(industry_data["products"])))
    product_materials = industry_data["materials"]

    # Генерируем цены на продукты
    prices = {}
    for prod in selected_products:
        prices[prod] = random.randint(50, 5000)

    # Добавляем цены на материалы
    for mat, price in material_prices.items():
        prices[mat] = price

    # Даты
    order_date = datetime(2025, 6, 1) + timedelta(days=random.randint(0, 30))
    shipment_date = order_date + timedelta(days=random.randint(2, 7))
    discount = round(random.uniform(0, 15), 1)
    vat = 20 if random.random() > 0.5 else 10

    # Заказчики
    customers = ['ООО "Ассоль"', 'ООО "Продукты Плюс"', 'ИП Петров А.В.', 'ООО "Молочный мир"', 
                 'ООО "Фермер"', 'ИП Сидорова Е.К.', 'ООО "Вкусняшка"', 'ООО "Здоровье"']
    customer = random.choice(customers)

    # ==================== 1. ЗАКАЗ ПОКУПАТЕЛЯ ====================
    order_rows = []
    total_sum = 0
    for i, prod in enumerate(selected_products, 1):
        qty = random.randint(5, 50)
        price = prices[prod]
        sum_prod = qty * price
        total_sum += sum_prod
        order_rows.append({"№": i, "Продукция": prod.strip(), "Кол-во": qty, "Ед. изм.": "шт", "Цена": price, "Сумма": sum_prod})
    order_rows.append({"№": "", "Продукция": "Итого:", "Кол-во": "", "Ед. изм.": "", "Цена": "", "Сумма": total_sum})
    df_order = pd.DataFrame(order_rows)
    with pd.ExcelWriter(f"{folder}/Заказ покупателя.xlsx", engine='openpyxl') as writer:
        df_order.to_excel(writer, index=False, startrow=3)
        ws = writer.sheets['Sheet1']
        ws['A1'] = f"Заказ покупателя № {var_id} от {order_date.strftime('%d %B %Y')} г."
        ws['A2'] = f"Исполнитель: ООО Молочный комбинат «Полесье»"
        ws['A3'] = f"Заказчик: {customer}"

    # ==================== 2. СПЕЦИФИКАЦИЯ ====================
    first_product = selected_products[0]
    spec_rows = []
    if first_product in product_materials:
        for mat, norm in product_materials[first_product]:
            spec_rows.append({"Материалы": mat.strip(), "Ед. изм.": "кг", "Количество": norm})
    df_spec = pd.DataFrame(spec_rows, columns=["Материалы", "Ед. изм.", "Количество"])
    with pd.ExcelWriter(f"{folder}/Спецификация.xlsx", engine='openpyxl') as writer:
        df_spec.to_excel(writer, index=False, startrow=4)
        ws = writer.sheets['Sheet1']
        ws['A1'] = f'Спецификация "{first_product.strip()}"'
        ws['A2'] = f"Продукция: {first_product.strip()}"
        ws['A3'] = "Количество: 1 шт."
        ws['A4'] = "Изготовитель: ООО Молочный комбинат «Полесье»"

    # ==================== 3. ПРОИЗВОДСТВО ====================
    prod_rows = [{"№": 1, "Наименование продукции": first_product.strip(), "Код": f"НФ-000000{var_id+5}", "Кол-во": 10, "Ед. изм.": "шт"}]
    df_prod = pd.DataFrame(prod_rows)

    mat_rows = []
    if first_product in product_materials:
        for i, (mat, norm) in enumerate(product_materials[first_product], 1):
            qty = 10 * norm
            mat_rows.append({"№": i, "Наименование материала": mat.strip(), "Код": f"НФ-000000{var_id+i+3}", "Кол-во": round(qty, 3), "Ед. изм.": "кг"})
    df_mat = pd.DataFrame(mat_rows)

    with pd.ExcelWriter(f"{folder}/Производство.xlsx", engine='openpyxl') as writer:
        df_prod.to_excel(writer, sheet_name='Sheet1', index=False, startrow=2)
        df_mat.to_excel(writer, sheet_name='Sheet1', index=False, startrow=7)
        ws = writer.sheets['Sheet1']
        ws['A1'] = f"Производство № {var_id} от {order_date.strftime('%d %B %Y')} г."
        ws['A2'] = "Продукция:"
        ws['A6'] = "Материалы:"

    # ==================== 4. РАСЧЕТ СТОИМОСТИ ====================
    cost_rows = []
    prod_price = prices[first_product]
    cost_rows.append({"Тип": "Продукция", "Ед. изм.": "шт", "Количество": 1.000, "Цена": "", "Стоимость": prod_price})
    total_materials = 0
    if first_product in product_materials:
        for mat, norm in product_materials[first_product]:
            mat_price = prices.get(mat, 100)
            mat_cost = norm * mat_price
            total_materials += mat_cost
            cost_rows.append({"Тип": mat.strip(), "Ед. изм.": "кг", "Количество": norm, "Цена": mat_price, "Стоимость": mat_cost})
    cost_rows.append({"Тип": "Итого", "Ед. изм.": "", "Количество": "", "Цена": "", "Стоимость": prod_price})
    df_cost = pd.DataFrame(cost_rows)
    with pd.ExcelWriter(f"{folder}/Расчет стоимости продукции.xlsx", engine='openpyxl') as writer:
        df_cost.to_excel(writer, index=False, startrow=0)
        writer.sheets['Sheet1'].column_dimensions['A'].width = 40

    # ==================== 5. ЦЕНЫ ====================
    price_rows = []
    for prod in selected_products:
        price_rows.append({"Продукция/Материалы": prod.strip(), "Цена": prices[prod]})
    if first_product in product_materials:
        for mat, _ in product_materials[first_product]:
            if mat in prices:
                price_rows.append({"Продукция/Материалы": mat.strip(), "Цена": prices[mat]})
    df_prices = pd.DataFrame(price_rows)
    with pd.ExcelWriter(f"{folder}/Цены.xlsx", engine='openpyxl') as writer:
        df_prices.to_excel(writer, index=False, startrow=0)
        writer.sheets['Sheet1'].column_dimensions['A'].width = 50

    print(f"✅ Вариант {var_id:02d} ({company_name}) - {industry_type}")
    print(f"   Продукты: {', '.join(selected_products)}")

# ==================== ГЕНЕРАЦИЯ ВСЕХ 30 ВАРИАНТОВ ====================
print("⏳ Генерация 30 вариантов с реалистичными данными...\n")
for i, name in enumerate(COMPANY_NAMES, 1):
    create_variant(i, name)
print("\n🎉 Готово! Все файлы сохранены в папках Variant_01_* ... Variant_30_*")
print("📁 Файлы созданы в формате XLSX (Excel)")