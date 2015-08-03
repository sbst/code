#pragma once
struct State{
	int num_of_state;
	int _1st_state;
	int _2nd_state;
};

class automat{
private:
	State current_state;
	State* states;
	double number;
public:
	automat(void);
	~automat(void);

	void send_signal(bool signal);
	void input_number(void);
	double automat::output_number(void);
	int ret_current_state(void);
};
