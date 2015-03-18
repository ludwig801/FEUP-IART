:-use_module(library(lists)).

% Estados
estado_inicial(b(0, 0)).
estado_final(b(2, 0)).

% Transições
sucessor(b(X,Y), b(4,Y)) :- X<4.
sucessor(b(X,Y), b(X,3)) :- Y<3.
sucessor(b(X,Y), b(0,Y)) :- X>0.
sucessor(b(X,Y), b(X,0)) :- Y>0.
sucessor(b(X,Y), b(4,Y1)) :-
			X+Y>=4,
			X<4,
			Y1 is Y-(4-X).
sucessor(b(X,Y), b(X1,3)) :-
			X+Y>=3,
			Y<3,
			X1 is X-(3-Y).
sucessor(b(X,Y), b(X1,0)) :-
			X+Y<4,
			Y>0,
			X1 is X+Y.
sucessor(b(X,Y), b(0,Y1)) :-
			X+Y<3,
			X>0,
			Y1 is X+Y.
			
			
% Pesquisa em profundidade

% Se já se encontra no estado final
pp(E, _, [E]) :- estado_final(E).


% Estado intermédio
pp(E, V, [E|R]) :- 
	sucessor(E, E2),
	\+ member(E2, V),
	pp(E2, [E2|V], R). 

solve_pp(S) :- 
	estado_inicial(E),
	pp(E, [E], S),
	write(S).
	
	
% Pesquisa em largura
pl([[E|Cam]|_], [E|Cam]) :- estado_final(E).

pl([[E|Cam]|R], Sol) :- 
	findall([E2|[E|Cam]], sucessor(E, E2), LS),
	append(R, LS, LR),
	pl(LR, Sol).
	
	
solve_pl(S) :- 
	estado_inicial(E),
	pl([[E|[]]], S),
	reverse(S, S1),
	write(S1).
	
	
			


