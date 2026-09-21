"""
AdvSearch - Laboratorio (PARTE I)
Autores:
 - Diego Alejandro Rozo
 - Juan Camilo Melo

Este archivo implementa:
 - `StateGame` (clase abstracta)
 - `TakeawayGame` (ejemplo simple de juego por turnos para 2 jugadores)
 - Ejemplos de uso (estados iniciales y generación de acciones)

Juego Takeaway:
 - Empieza con N fichas.
 - Cada turno un jugador puede quitar 1 o 2 fichas (si es posible).
 - Quien quite la última ficha gana.

Nota: diseñado para integrarse fácilmente con un motor Minimax/AlphaBeta.
"""
from __future__ import annotations
from abc import ABC, abstractmethod
from dataclasses import dataclass
from typing import List, Tuple
import math

class StateGame(ABC):
    """Abstracta: define los métodos clave para modelar cualquier juego por turnos."""

    @classmethod
    @abstractmethod
    def start(cls) -> "StateGame":
        """Devuelve el estado inicial del juego."""
        raise NotImplementedError()

    @abstractmethod
    def actionResults(self) -> List["StateGame"]:
        """Listar los estados resultantes de acciones legales desde este estado."""
        raise NotImplementedError()

    @abstractmethod
    def isTerminal(self) -> bool:
        """¿Es un estado terminal (fin de juego)?"""
        raise NotImplementedError()

    @abstractmethod
    def toMove(self) -> str:
        """Identificador del jugador que tiene el turno en este estado."""
        raise NotImplementedError()

    @staticmethod
    def rival(player: str, players: Tuple[str, str]) -> str:
        """Devuelve el rival dado el jugador y la tupla de jugadores."""
        if players[0] == player:
            return players[1]
        if players[1] == player:
            return players[0]
        # si no coincide, devolver el otro por defecto
        return players[1]

    @abstractmethod
    def heuristicUtility(self, player: str) -> int:
        """Heurística/valor de utilidad para `player` en este estado."""
        raise NotImplementedError()

    @abstractmethod
    def toString(self) -> str:
        """Representación en cadena del estado."""
        raise NotImplementedError()


@dataclass(frozen=True)
class TakeawayGame(StateGame):
    """Juego sencillo: pila de fichas. Cada turno quitar 1 o 2. Quien quite la última ficha gana.

    Atributos:
      remaining: fichas restantes
      player: identificador del jugador que tiene el turno
      players: tupla con (playerA, playerB)
    """
    remaining: int
    player: str
    players: Tuple[str, str] = ("MAX", "MIN")
    max_remove: int = 2

    @classmethod
    def start(cls, initial: int = 5, start_player: str = "MAX", players: Tuple[str, str] = ("MAX","MIN")) -> "TakeawayGame":
        return cls(remaining=initial, player=start_player, players=players)

    def actionResults(self) -> List["TakeawayGame"]:
        res: List[TakeawayGame] = []
        for take in range(1, self.max_remove + 1):
            if take <= self.remaining:
                next_state = TakeawayGame(
                    remaining=self.remaining - take,
                    player=StateGame.rival(self.player, self.players),
                    players=self.players,
                    max_remove=self.max_remove,
                )
                res.append(next_state)
        return res

    def isTerminal(self) -> bool:
        return self.remaining == 0

    def toMove(self) -> str:
        return self.player

    def heuristicUtility(self, player: str) -> int:
        # Si terminal: ganador obtiene gran valor positivo, perdedor negativo
        if self.isTerminal():
            # el jugador que no tiene el turno en un estado con remaining==0 fue quien tomó la última ficha
            winner = StateGame.rival(self.player, self.players)
            return 10_000 if winner == player else -10_000
        # Heurística simple (para estados no terminales):
        # menos fichas restantes favorece al que va a mover en próximos turnos
        # devolvemos una estimación relativa: (si mi turno) +remaining, sino -remaining
        if self.player == player:
            return self.remaining
        else:
            return -self.remaining

    def toString(self) -> str:
        return f"Takeaway(remaining={self.remaining}, toMove={self.player})"

    def __repr__(self) -> str:
        return self.toString()


# Ejemplos de uso (PARTE I)
if __name__ == "__main__":
    # Nombres de los integrantes (solicitados)
    integrantes = ("Diego Alejandro Rozo", "Juan Camilo Melo")

    # Definimos los identificadores de jugador visibles
    players = (integrantes[0], integrantes[1])

    print("== PARTE I: Juego Takeaway (dos jugadores) ==")
    s0 = TakeawayGame.start(initial=5, start_player=players[0], players=players)
    print("Estado inicial:", s0)

    # Mostrar acciones legales desde el estado inicial
    actions = s0.actionResults()
    print("Acciones legales desde el estado inicial:")
    for a in actions:
        print("  ->", a, "terminal=", a.isTerminal(), "heuristic(for ", players[0], ")=", a.heuristicUtility(players[0]))

    # Simular un pequeño avance: tomar la primera acción y listar sus acciones
    if actions:
        s1 = actions[0]
        print("\nEstado tras primera acción elegida:", s1)
        print("Siguiente acciones:")
        for a in s1.actionResults():
            print("   ->", a, "terminal=", a.isTerminal())

    print("\nHeurística del estado inicial para", players[0], ":", s0.heuristicUtility(players[0]))
    print("Heurística del estado inicial para", players[1], ":", s0.heuristicUtility(players[1]))

    print("\nListo: `StateGame` y `TakeawayGame` implementados. Puede integrarse con un motor Minimax/AlphaBeta.")
