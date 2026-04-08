import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import time

base_dir = os.path.dirname(__file__)
csv_path = os.path.join(base_dir, "../../../results_herbivore.csv")

df = None

for _ in range(5):
    try:
        df = pd.read_csv(csv_path, sep=';', decimal=',')
        break
    except Exception:
        time.sleep(0.5)

#df = pd.read_csv("../../../results_herbivore.csv", sep=';', decimal=',')

if df is None:
    raise Exception("Не удалось прочитать CSV файл")

#df = pd.read_csv(csv_path, sep=';', decimal=',')
df = df[df['Generation'] >= 1].copy()

#---------

reset_idx = df[df['Generation'].diff() < 0].index
if len(reset_idx) > 0:
    first_run = df.loc[:reset_idx[0]-1].copy()
else:
    first_run = df.copy()

#---------

avg_columns = [col for col in first_run.columns if col.startswith("AvgFitness")]

#---------

plt.figure(figsize=(10, 6))

line_styles = ['-', '--', '-.']

for i, col in enumerate(avg_columns):
    plt.plot(
        first_run['Generation'],
        first_run[col],
        linestyle=line_styles[i % len(line_styles)],
        label=col
    )

plt.xlabel("Generation - Herbivores")
plt.ylabel("Value")
plt.title("Evolution of Average Fitness")

# Верхняя граница Y жестко, нижняя автоматическая
#plt.ylim(bottom=None, top=101)

# Сетка Y
y_min = first_run[avg_columns].min().min()
y_max = first_run[avg_columns].max().max()

plt.ylim(y_min, y_max)

#y_ticks = np.arange(np.floor(y_min), 101, 2.5)
y_ticks = np.arange(
    np.floor(y_min / 2.5) * 2.5,
    np.ceil(y_max / 2.5) * 2.5 + 2.5,
    2.5
)
plt.yticks(y_ticks)
plt.grid(axis='y', color='gray', linestyle='--', linewidth=0.5, alpha=0.3)

# --- Динамические деления по X ---
x_min = first_run['Generation'].min()
x_max = first_run['Generation'].max()

# Шаг делений по X
if x_max <= 50:
    step = 2
elif x_max <= 100:
    step = 4
elif x_max <= 200:
    step = 8
else:
    step = max(1, int(x_max * 0.1))

x_ticks = np.arange(x_min, x_max + 1, step)
plt.xticks(x_ticks)
plt.grid(axis='x', color='gray', linestyle='--', linewidth=0.5, alpha=0.3)

plt.legend()
plt.show()