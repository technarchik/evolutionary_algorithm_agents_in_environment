import matplotlib
matplotlib.use('TkAgg')  # принудительно включаем GUI backend

import matplotlib.pyplot as plt

x = [0, 1, 2, 3, 4]
y = [i**2 for i in x]

plt.plot(x, y)
plt.title("Test Plot")

plt.show()