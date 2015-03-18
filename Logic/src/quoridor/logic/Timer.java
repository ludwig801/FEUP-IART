package quoridor.logic;

public class Timer {

	private long startTime;
	private long runTime;
	
	public Timer() {
		startTime = 0;
	}
	
	public void start() {
		startTime = System.nanoTime();
		runTime = startTime;
	}
	
	public void restart() {
		start();
	}
	
	public long getParcial() {
		long current = System.nanoTime();
		long temp = current - runTime;
		runTime = current;
		return temp;
	}
	
	public long stop() {
		return getParcial();
	}
	
	public long getTotal() {
		return System.nanoTime() - startTime;
	}
}
