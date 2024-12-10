import os
import pandas as pd
import matplotlib.pyplot as plt

def read_data_from_file(filnamn):
    """
    Läs data från en textfil och returnera ett DataFrame med relevanta data samt metadata.
    """
    if not os.path.exists(filnamn):
        print(f"Filen '{filnamn}' hittades inte.")
        return None, None, None, None

    try:
        # Läs alla rader från filen
        with open(filnamn, 'r') as f:
            lines = f.read().strip().split('\n')

        # Initialisera variabler
        pileholder_counts = {}  # Dynamisk räknare för alla spelare som är "Pileholder"
        rounds = []  # För att hålla reda på rundnummer
        meta_data = {}
        points = {}  # Dictionary för att lagra poäng för varje spelare
        player_types = {}  # För att lagra typ av spelare (bot, human, player)
        current_round = 0  # För att hålla reda på vilken runda vi är på

        # Iterera över varje rad och hämta relevant information
        for line in lines:
            if line.startswith('Time:'):
                meta_data['Time'] = line.split(': ')[1]
            elif line.startswith('Seed:'):
                meta_data['Seed'] = line.split(': ')[1]
            elif line.startswith('Extra Information:'):
                meta_data['Extra Information'] = line.split(': ')[1]
            elif line.startswith('PlayerID:'):
                player_ids = line.split(':')[1].strip().split()  # Läs alla PlayerID från raden
                for i, player_id in enumerate(player_ids):
                    player_id = int(player_id)  # Omvandla till int för att jämföra
                    # Lägg till spelarens typ baserat på PlayerID
                    player_types[i + 1] = "Bot" if player_id == 1 else "Human" if player_id == 0 else "Player"
            elif line.startswith('Points:'):
                points_data = list(map(int, line.split(': ')[1].split(',')))

                # Dynamiskt skapa en lista för poäng för varje spelare
                for i, point in enumerate(points_data, start=1):
                    if i not in points:
                        points[i] = []  # Om spelaren inte finns, skapa en lista för den
                    points[i].append(point)

                # Lägg till runda
                rounds.append(current_round)
                current_round += 1

            elif line.startswith('Pileholder:'):
                pileholder = int(line.split(': ')[1])
                if pileholder not in pileholder_counts:
                    pileholder_counts[pileholder] = 0
                pileholder_counts[pileholder] += 1  # Öka räknaren för den spelare som är Pileholder

        # Skapa en DataFrame från poängen varje runda
        data = pd.DataFrame({'Round': rounds})
        for player, player_points in points.items():
            data[f'Player {player}'] = player_points

        # Vänd datan så att den senaste rundan kommer först
        data = data.iloc[::-1].reset_index(drop=True)

        # Uppdatera round-numreringen så att det börjar från 0 efter omvändning
        data['Round'] = range(0, len(data))  # Start from 0 instead of 1

        return data, meta_data, pileholder_counts, player_types

    except Exception as e:
        print(f"Fel vid läsning av filen '{filnamn}': {e}")
        return None, None, None, None


def combine_data(files):
    """
    Kombinera data från flera filer och beräkna genomsnittet för varje runda.
    Anpassad för att hantera filer med olika antal rundor.
    Poäng för avslutade matcher fylls med senaste tillgängliga poäng.
    """
    all_data = []
    pileholder_counts_total = {}
    player_types_total = {}
    max_rounds = 0  # För att spåra det största antalet rundor

    # Läs data från varje fil
    for filnamn in files:
        print(f"Bearbetar fil: {filnamn}")
        data, _, pileholder_counts, player_types = read_data_from_file(filnamn)
        
        if data is None:
            print(f"Fel vid läsning av filen '{filnamn}'. Hoppar över denna fil.")
            continue

        all_data.append(data)

        # Lägg till pileholder counts
        for player, count in pileholder_counts.items():
            pileholder_counts_total[player] = pileholder_counts_total.get(player, 0) + count

        # Lägg till player types
        player_types_total.update(player_types)

        # Uppdatera max_rounds
        max_rounds = max(max_rounds, len(data))

    if not all_data:
        print("Ingen data kunde kombineras från filerna.")
        return None, None, None

    # Fyll alla dataframes med NaN för att matcha det största antalet rundor
    for i, data in enumerate(all_data):
        if len(data) < max_rounds:
            # Lägg till NaN-värden för att matcha det största antalet rundor
            all_data[i] = pd.concat([data, pd.DataFrame({'Round': range(len(data), max_rounds)})], ignore_index=True)

        # Fyll saknade värden med senaste tillgängliga värden (håller poäng konstant efter matchens slut)
        all_data[i] = all_data[i].fillna(method='ffill')

    # Beräkna genomsnittet för varje runda för varje spelare
    avg_data = pd.DataFrame({'Round': range(max_rounds)})
    
    # Bestäm ordningen för spelarna (Bot 1, Human 2, Bot 3, Bot 4)
    player_order = [1, 2, 3, 4]
    
    for player in player_order:
        avg_data[f'Player {player}'] = [
            sum(df[f'Player {player}'][i] if f'Player {player}' in df.columns and i < len(df) else 0 for df in all_data) / len(all_data) 
            for i in range(max_rounds)
        ]

    return avg_data, pileholder_counts_total, player_types_total


def plot_graph(data, pileholder_counts, player_types):
    """
    Plottar två grafer:
    1. Punktdiagram som visar poängen varje spelare fick just den aktuella rundan.
    2. Stapeldiagram som visar hur många gånger varje spelare har varit "Pileholder".
    """
    # Kontrollera om datan är giltig
    if data is None or data.empty:
        print("Ingen giltig data att plotta.")
        return

    # Skapa ett punktdiagram för varje spelares poäng per runda
    plt.figure(figsize=(14, 8))

    # Plot 1: Poäng per runda
    plt.subplot(2, 1, 1)  # Två rader, en kolumn, första subplot
    colors = ['red', 'green', 'blue', 'purple']  # Färger för varje spelare
    player_order = [1, 2, 3, 4]  # Spelarnas ordning (Bot 1, Human 2, Bot 3, Bot 4)
    
    for i, player in enumerate(player_order):  # Hantera varje spelare
        player_column = f'Player {player}'
        player_type = player_types.get(player, "Player")  # Hämta spelarens typ
        plt.plot(data['Round'], data[player_column], marker='o', label=f'Bot {player}' if player_type == "Bot" else f'Human {player}', color=colors[i])

    plt.title('Genomsnittliga poäng för varje spelare per runda')
    plt.xlabel('Rundor')
    plt.ylabel('Genomsnittlig poäng denna runda')
    plt.grid(True)
    plt.legend()  # Lägg till en legend i diagrammet
    plt.tight_layout()  # Justera layouten för att undvika att etiketter klipps bort

    # Plot 2: Antal gånger varje spelare har varit "Pileholder"
    plt.subplot(2, 1, 2)  # Två rader, en kolumn, andra subplot
    players = [f'Bot {i}' if player_types.get(i, "Player") == "Bot" else f'Human {i}' for i in player_order]
    pileholder_values = [pileholder_counts.get(player, 0) for player in player_order]
    
    plt.bar(players, pileholder_values, color=colors)

    plt.title('Antal gånger varje spelare har varit Pileholder')
    plt.xlabel('Spelare')
    plt.ylabel('Antal gånger som Pileholder')
    plt.grid(axis='y')

    # Justera layouten igen för att undvika överlappning
    plt.tight_layout()

    # Visa alla diagram
    plt.show()

def main():
    # Fråga användaren om vilka filer som ska användas
    file_names = input("Ange filnamn (separera med kommatecken för flera filer): ").split(',')
    
    # Ta bort extra mellanslag från filnamnen och lägg till filtillägget ".txt"
    file_names = [f.strip() + ".txt" for f in file_names]
    print(f"Filer som kommer att läsas in: {file_names}")

    # Kontrollera att alla filer finns innan vi fortsätter
    for file in file_names:
        if not os.path.exists(file):
            print(f"Filen '{file}' finns inte. Kontrollera sökvägen.")
            return  # Avsluta om någon fil inte finns

    # Kombinera data från alla filer och beräkna genomsnitt
    avg_data, pileholder_counts_total, player_types_total = combine_data(file_names)

    if avg_data is not None:
        # Skriv ut genomsnittlig data i konsolen
        print("\nGenomsnittlig poäng per runda:")
        print(avg_data)

        # Plotta diagrammen
        plot_graph(avg_data, pileholder_counts_total, player_types_total)
    else:
        print("Det gick inte att kombinera data från filerna.")

if __name__ == "__main__":
    main()