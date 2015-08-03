//#include "StdAfx.h"
#include "automat.h"

automat::automat(void){
	states = new State[5];
	
	states[0].num_of_state = 1;
	states[0]._1st_state = 2;
	states[0]._2nd_state = 1;

	states[1].num_of_state = 2;
	states[1]._1st_state = 3;
	states[1]._2nd_state = 4;

	states[2].num_of_state = 3;
	states[2]._1st_state = 1;
	states[2]._2nd_state = 4;

	states[3].num_of_state = 4;
	states[3]._1st_state = 4;
	states[3]._2nd_state = 0;

	states[4].num_of_state = 5;
	states[4]._1st_state = NULL;
	states[4]._2nd_state = NULL;

	current_state = states[0];
}

automat::~automat(void){
	delete []states;
}

void automat::input_number(){
	std::cout << "Input number: ";
	std::cin >> number;
}

double automat::output_number(){
	return number;
}

void automat::send_signal(bool signal){
	if(signal == true){
		switch(current_state._1st_state){
		case 0:
			input_number();
			Sleep(10);
			current_state = states[0];
			break;
		case 1:
			number = number + number;
			Sleep(10);
			current_state = states[1];
			break;
		case 2:
			number = number * number;
			Sleep(10);
			current_state = states[2];
			break;
		case 3:
			number = (number * number + 2 * number)/number;
			Sleep(10);
			current_state = states[3];
			break;
		case 4:
			std::cout << "Result calculating is " << output_number() << std::endl;
			Sleep(10);
 			current_state = states[4];
			break;
		default:
			Sleep(10);
			break;
		}
	}
	else{
		switch(current_state._2nd_state){
		case 0:
			input_number();
			Sleep(10);
			current_state = states[0];
			break;
		case 1:
			number = number + number;
			Sleep(10);
			current_state = states[1];
			break;
		case 2:
			number = number * number;
			Sleep(10);
			current_state = states[2];
			break;
		case 3:
			number = (number * number + 2 * number)/number;
			Sleep(10);
			current_state = states[3];
			break;
		case 4:
			std::cout << "Result calculating is " << output_number() << std::endl;
			Sleep(10);
 			current_state = states[4];
			break;
		default:
			Sleep(10);
			break;
		}
	}
}

int automat::ret_current_state(void){
	return current_state.num_of_state;
}
