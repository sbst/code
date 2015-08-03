// CUTMACHINEGUN.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"
#include <Windows.h>
#include <iostream>
#include "..\automat\automat.cpp"

int _tmain()
{
	automat new_automat;
	int signal;
	long long before, after;//, quantity = 1000000000;	
	std::cout << "Current state is " << new_automat.ret_current_state() << std::endl;
	std::cout << GetTickCount() << std::endl;
	new_automat.input_number();
	while(new_automat.ret_current_state() != 5){
		std::cout << "Pleate input state of signal:\n";
		std::cin >> signal;
		if(signal == 1 || signal == 0){
			before = GetTickCount();
			new_automat.send_signal(signal);
			after = GetTickCount();
			std::cout << "Current state is " << new_automat.ret_current_state() << std::endl << "Time: " << (after-before) << "tiks (in standart ms) " << std::endl;
		}
		signal = NULL;
	}
	std::cout << "Life is over\n";
	system("PAUSE");
	return 0;
}

