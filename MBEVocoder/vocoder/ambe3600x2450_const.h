/**
* Digital Voice Modem - MBE Vocoder
* GPLv2 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / MBE Vocoder
*
*/
/*
 * Copyright (C) 2010 mbelib Author
 * GPG Key ID: 0xEA5EFE2C (9E7A 5527 9CDC EBF7 BF1B  D772 4F98 E863 EA5E FE2C)
 *
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND ISC DISCLAIMS ALL WARRANTIES WITH
 * REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
 * AND FITNESS.  IN NO EVENT SHALL ISC BE LIABLE FOR ANY SPECIAL, DIRECT,
 * INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
 * LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE
 * OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
 * PERFORMANCE OF THIS SOFTWARE.
 */
#if !defined(__AMBE3600x2450_CONST_H__)
#define __AMBE3600x2450_CONST_H__

#ifdef _MSC_VER
#pragma warning(disable: 4305)
#endif

// ---------------------------------------------------------------------------
//  Constants
// ---------------------------------------------------------------------------

/*
 * Fundamental Frequency Quanitization Table 
 */

const float AmbeW0table[120] = {
    0.049971, 0.049215, 0.048471, 0.047739, 0.047010, 0.046299,
    0.045601, 0.044905, 0.044226, 0.043558, 0.042900, 0.042246,
    0.041609, 0.040979, 0.040356, 0.039747, 0.039148, 0.038559,
    0.037971, 0.037399, 0.036839, 0.036278, 0.035732, 0.035198,
    0.034672, 0.034145, 0.033636, 0.033133, 0.032635, 0.032148,
    0.031670, 0.031122, 0.030647, 0.030184, 0.029728, 0.029272,
    0.028831, 0.028395, 0.027966, 0.027538,
    0.027122, 0.026712, 0.026304, 0.025906, 0.025515, 0.025129,
    0.024746, 0.024372, 0.024002, 0.023636, 0.023279, 0.022926,
    0.022581, 0.022236, 0.021900, 0.021570, 0.021240, 0.020920,
    0.020605, 0.020294, 0.019983, 0.019684, 0.019386, 0.019094,
    0.018805, 0.018520, 0.018242, 0.017965, 0.017696, 0.017431,
    0.017170, 0.016911, 0.016657, 0.016409, 0.016163, 0.015923,
    0.015686, 0.015411, 0.015177, 0.014946,
    0.014721, 0.014496, 0.014277, 0.014061, 0.013847, 0.013636,
    0.013430, 0.013227, 0.013025, 0.012829, 0.012634, 0.012444,
    0.012253, 0.012068, 0.011887, 0.011703, 0.011528, 0.011353,
    0.011183, 0.011011, 0.010845, 0.010681, 0.010517, 0.010359,
    0.010202, 0.010050, 0.009895, 0.009747, 0.009600, 0.009453,
    0.009312, 0.009172, 0.009033, 0.008896, 0.008762, 0.008633,
    0.008501, 0.008375, 0.008249, 0.008125
};

const float AmbeLtable[120] = {
    9, 9, 9, 9, 9, 9,
    10, 10, 10, 10, 10, 10,
    11, 11, 11, 11, 11, 11,
    12, 12, 12, 12, 12, 13,
    13, 13, 13, 13, 14, 14,
    14, 14, 15, 15, 15, 15,
    16, 16, 16, 16, 17, 17,
    17, 17, 18, 18, 18, 18,
    19, 19, 19, 20, 20, 20,
    21, 21, 21, 22, 22, 22,
    23, 23, 23, 24, 24, 24,
    25, 25, 26, 26, 26, 27,
    27, 28, 28, 29, 29, 30,
    30, 30, 31, 31, 32, 32,
    33, 33, 34, 34, 35, 36,
    36, 37, 37, 38, 38, 39,
    40, 40, 41, 42, 42, 43,
    43, 44, 45, 46, 46, 47,
    48, 48, 49, 50, 51, 52,
    52, 53, 54, 55, 56, 56
};

/*
 * V/UV Quantization Vectors
 */
const int AmbeVuv[32][8] = {
    {1, 1, 1, 1, 1, 1, 1, 1},
    {1, 1, 1, 1, 1, 1, 1, 1},
    {1, 1, 1, 1, 1, 1, 1, 0},
    {1, 1, 1, 1, 1, 1, 1, 1},
    {1, 1, 1, 1, 1, 1, 0, 0},
    {1, 1, 0, 1, 1, 1, 1, 1},
    {1, 1, 1, 0, 1, 1, 1, 1},
    {1, 1, 1, 1, 1, 0, 1, 1},
    {1, 1, 1, 1, 0, 0, 0, 0},
    {1, 1, 1, 1, 1, 0, 0, 0},
    {1, 1, 1, 0, 0, 0, 0, 0},
    {1, 1, 1, 0, 0, 0, 0, 1},
    {1, 1, 0, 0, 0, 0, 0, 0},
    {1, 1, 1, 0, 0, 0, 0, 0},
    {1, 0, 0, 0, 0, 0, 0, 0},
    {1, 1, 1, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0}
};

/*
 * V/UV Quantization Vectors
 * alternate version
 */
/*
const int AmbeVuv[32][8] = {
    {1, 1, 1, 1, 1, 1, 1, 1},
    {1, 1, 1, 1, 1, 1, 1, 1},
    {1, 1, 1, 1, 1, 1, 1, 0},
    {1, 1, 1, 1, 1, 1, 1, 0},
    {1, 1, 1, 1, 1, 1, 0, 0},
    {1, 1, 0, 1, 1, 1, 1, 1},
    {1, 1, 1, 0, 1, 1, 1, 1},
    {1, 1, 1, 1, 1, 0, 1, 1},
    {1, 1, 1, 1, 0, 0, 0, 0},
    {1, 1, 1, 1, 1, 0, 0, 0},
    {1, 1, 1, 0, 0, 0, 0, 0},
    {1, 1, 1, 0, 0, 0, 0, 1},
    {1, 1, 0, 0, 0, 0, 0, 0},
    {1, 1, 0, 0, 0, 0, 0, 0},
    {1, 0, 0, 0, 0, 0, 0, 0},
    {1, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0},
    {0, 0, 0, 0, 0, 0, 0, 0}
};
*/

/*
 * Log Magnitude Prediction Residual Block Lengths
 */
const int AmbeLmprbl[57][4] = {
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {0, 0, 0, 0,},
    {2, 2, 2, 3,},
    {2, 2, 3, 3,},
    {2, 3, 3, 3,},
    {2, 3, 3, 4,},
    {3, 3, 3, 4,},
    {3, 3, 4, 4,},
    {3, 3, 4, 5,},
    {3, 4, 4, 5,},
    {3, 4, 5, 5,},
    {4, 4, 5, 5,},
    {4, 4, 5, 6,},
    {4, 4, 6, 6,},
    {4, 5, 6, 6,},
    {4, 5, 6, 7,},
    {5, 5, 6, 7,},
    {5, 5, 7, 7,},
    {5, 6, 7, 7,},
    {5, 6, 7, 8,},
    {5, 6, 8, 8,},
    {6, 6, 8, 8,},
    {6, 6, 8, 9,},
    {6, 7, 8, 9,},
    {6, 7, 9, 9,},
    {6, 7, 9, 10,},
    {7, 7, 9, 10,},
    {7, 8, 9, 10,},
    {7, 8, 10, 10,},
    {7, 8, 10, 11,},
    {8, 8, 10, 11,},
    {8, 9, 10, 11,},
    {8, 9, 11, 11,},
    {8, 9, 11, 12,},
    {8, 9, 11, 13,},
    {8, 9, 12, 13,},
    {8, 10, 12, 13,},
    {9, 10, 12, 13,},
    {9, 10, 12, 14,},
    {9, 10, 13, 14,},
    {9, 11, 13, 14,},
    {10, 11, 13, 14,},
    {10, 11, 13, 15,},
    {10, 11, 14, 15,},
    {10, 12, 14, 15,},
    {10, 12, 14, 16,},
    {11, 12, 14, 16,},
    {11, 12, 15, 16,},
    {11, 12, 15, 17,},
    {11, 13, 15, 17}
};

/*
 * Gain Quantizer Levels
 */
const float AmbeDg[32] = {
    -2.0, -0.67, 0.297941, 0.663728, 1.036829, 1.438136, 1.890077, 2.227970,
    2.478289, 2.667544, 2.793619, 2.893261, 3.020630, 3.138586, 3.237579, 3.322570,
    3.432367, 3.571863, 3.696650, 3.814917, 3.920932, 4.022503, 4.123569, 4.228291,
    4.370569, 4.543700, 4.707695, 4.848879, 5.056757, 5.326468, 5.777581, 6.874496
};

/*
 * PRBA24 Vector Quantizer Levels
 */
const float AmbePRBA24[512][3] = {
    {0.526055, -0.328567, -0.304727},
    {0.441044, -0.303127, -0.201114},
    {1.030896, -0.324730, -0.397204},
    {0.839696, -0.351933, -0.224909},
    {0.272958, -0.176118, -0.098893},
    {0.221466, -0.160045, -0.061026},
    {0.496555, -0.211499, 0.047305},
    {0.424376, -0.223752, 0.069911},
    {0.264531, -0.353355, -0.330505},
    {0.273650, -0.253004, -0.250241},
    {0.484531, -0.297627, -0.071051},
    {0.410814, -0.224961, -0.084998},
    {0.039519, -0.252904, -0.115128},
    {0.017423, -0.296519, -0.045921},
    {0.225113, -0.224371, 0.037882},
    {0.183424, -0.260492, 0.050491},
    {0.308704, -0.073205, -0.405880},
    {0.213125, -0.101632, -0.333208},
    {0.617735, -0.137299, -0.213670},
    {0.514382, -0.126485, -0.170204},
    {0.130009, -0.076955, -0.229303},
    {0.061740, -0.108259, -0.203887},
    {0.244473, -0.110094, -0.051689},
    {0.230452, -0.076147, -0.028190},
    {0.059837, -0.254595, -0.562704},
    {0.011630, -0.135223, -0.432791},
    {0.207077, -0.152248, -0.148391},
    {0.158078, -0.128800, -0.122150},
    {-0.265982, -0.144742, -0.199894},
    {-0.356479, -0.204740, -0.156465},
    {0.000324, -0.139549, -0.066471},
    {0.001888, -0.170557, -0.025025},
    {0.402913, -0.581478, -0.274626},
    {0.191289, -0.540335, -0.193040},
    {0.632914, -0.401410, -0.006636},
    {0.471086, -0.463144, 0.061489},
    {0.044829, -0.438487, 0.033433},
    {0.015513, -0.539475, -0.006719},
    {0.336218, -0.351311, 0.214087},
    {0.239967, -0.380836, 0.157681},
    {0.347609, -0.901619, -0.688432},
    {0.064067, -0.826753, -0.492089},
    {0.303089, -0.396757, -0.108446},
    {0.235590, -0.446122, 0.006437},
    {-0.236964, -0.652532, -0.135520},
    {-0.418285, -0.793014, -0.034730},
    {-0.038262, -0.516984, 0.273681},
    {-0.037419, -0.958198, 0.214749},
    {0.061624, -0.238233, -0.237184},
    {-0.013944, -0.235704, -0.204811},
    {0.286428, -0.210542, -0.029587},
    {0.257656, -0.261837, -0.056566},
    {-0.235852, -0.310760, -0.165147},
    {-0.334949, -0.385870, -0.197362},
    {0.094870, -0.241144, 0.059122},
    {0.060177, -0.225884, 0.031140},
    {-0.301184, -0.306545, -0.446189},
    {-0.293528, -0.504146, -0.429844},
    {-0.055084, -0.379015, -0.125887},
    {-0.115434, -0.375008, -0.059939},
    {-0.777425, -0.592163, -0.107585},
    {-0.950500, -0.893847, -0.181762},
    {-0.259402, -0.396726, 0.010357},
    {-0.368905, -0.449026, 0.038299},
    {0.279719, -0.063196, -0.184628},
    {0.255265, -0.067248, -0.121124},
    {0.458433, -0.103777, 0.010074},
    {0.437231, -0.092496, -0.031028},
    {0.082265, -0.028050, -0.041262},
    {0.045920, -0.051719, -0.030155},
    {0.271149, -0.043613, 0.112085},
    {0.246881, -0.065274, 0.105436},
    {0.056590, -0.117773, -0.142283},
    {0.058824, -0.104418, -0.099608},
    {0.213781, -0.111974, 0.031269},
    {0.187554, -0.070340, 0.011834},
    {-0.185701, -0.081106, -0.073803},
    {-0.266112, -0.074133, -0.085370},
    {-0.029368, -0.046490, 0.124679},
    {-0.017378, -0.102882, 0.140482},
    {0.114700, 0.092738, -0.244271},
    {0.072922, 0.007863, -0.231476},
    {0.270022, 0.031819, -0.094208},
    {0.254403, 0.024805, -0.050389},
    {-0.182905, 0.021629, -0.168481},
    {-0.225864, -0.010109, -0.130374},
    {0.040089, 0.013969, 0.016028},
    {0.001442, 0.010551, 0.032942},
    {-0.287472, -0.036130, -0.296798},
    {-0.332344, -0.108862, -0.342196},
    {0.012700, 0.022917, -0.052501},
    {-0.040681, -0.001805, -0.050548},
    {-0.718522, -0.061234, -0.278820},
    {-0.879205, -0.213588, -0.303508},
    {-0.234102, -0.065407, 0.013686},
    {-0.281223, -0.076139, 0.046830},
    {0.141967, -0.193679, -0.055697},
    {0.100318, -0.161222, -0.063062},
    {0.265859, -0.132747, 0.078209},
    {0.244805, -0.139776, 0.122123},
    {-0.121802, -0.179976, 0.031732},
    {-0.185318, -0.214011, 0.018117},
    {0.047014, -0.153961, 0.218068},
    {0.047305, -0.187402, 0.282114},
    {-0.027533, -0.415868, -0.333841},
    {-0.125886, -0.334492, -0.290317},
    {-0.030602, -0.190918, 0.097454},
    {-0.054936, -0.209948, 0.158977},
    {-0.507223, -0.295876, -0.217183},
    {-0.581733, -0.403194, -0.208936},
    {-0.299719, -0.289679, 0.297101},
    {-0.363169, -0.362718, 0.436529},
    {-0.124627, -0.042100, -0.157011},
    {-0.161571, -0.092846, -0.183636},
    {0.084520, -0.100217, -0.000901},
    {0.055655, -0.136381, 0.032764},
    {-0.545087, -0.197713, -0.026888},
    {-0.662772, -0.179815, 0.026419},
    {-0.165583, -0.148913, 0.090382},
    {-0.240772, -0.182830, 0.105474},
    {-0.576315, -0.359473, -0.456844},
    {-0.713430, -0.554156, -0.476739},
    {-0.275628, -0.223640, -0.051584},
    {-0.359501, -0.230758, -0.027006},
    {-1.282559, -0.284807, -0.233743},
    {-1.060476, -0.399911, -0.562698},
    {-0.871952, -0.272197, 0.016126},
    {-0.747922, -0.329404, 0.276696},
    {0.643086, 0.046175, -0.660078},
    {0.738204, -0.127844, -0.433708},
    {1.158072, 0.025571, -0.177856},
    {0.974840, -0.009417, -0.112337},
    {0.418014, 0.032741, -0.124545},
    {0.381422, -0.001557, -0.085504},
    {0.768280, 0.056085, 0.095375},
    {0.680004, 0.052035, 0.152318},
    {0.473182, 0.012560, -0.264221},
    {0.345153, 0.036627, -0.248756},
    {0.746238, -0.025880, -0.106050},
    {0.644319, -0.058256, -0.095133},
    {0.185924, -0.022230, -0.070540},
    {0.146068, -0.009550, -0.057871},
    {0.338488, 0.013022, 0.069961},
    {0.298969, 0.047403, 0.052598},
    {0.346002, 0.256253, -0.380261},
    {0.313092, 0.163821, -0.314004},
    {0.719154, 0.103108, -0.252648},
    {0.621429, 0.172423, -0.265180},
    {0.240461, 0.104684, -0.202582},
    {0.206946, 0.139642, -0.138016},
    {0.359915, 0.101273, -0.052997},
    {0.318117, 0.125888, -0.003486},
    {0.150452, 0.050219, -0.409155},
    {0.188753, 0.091894, -0.325733},
    {0.334922, 0.029098, -0.098587},
    {0.324508, 0.015809, -0.135408},
    {-0.042506, 0.038667, -0.208535},
    {-0.083003, 0.094758, -0.174054},
    {0.094773, 0.102653, -0.025701},
    {0.063284, 0.118703, -0.000071},
    {0.355965, -0.139239, -0.191705},
    {0.392742, -0.105496, -0.132103},
    {0.663678, -0.204627, -0.031242},
    {0.609381, -0.146914, 0.079610},
    {0.151855, -0.132843, -0.007125},
    {0.146404, -0.161917, 0.024842},
    {0.400524, -0.135221, 0.232289},
    {0.324931, -0.116605, 0.253458},
    {0.169066, -0.215132, -0.185604},
    {0.128681, -0.189394, -0.160279},
    {0.356194, -0.116992, -0.038381},
    {0.342866, -0.144687, 0.020265},
    {-0.065545, -0.202593, -0.043688},
    {-0.124296, -0.260225, -0.035370},
    {0.083224, -0.235149, 0.153301},
    {0.046256, -0.309608, 0.190944},
    {0.187385, -0.008168, -0.198575},
    {0.190401, -0.018699, -0.136858},
    {0.398009, -0.025700, -0.007458},
    {0.346948, -0.022258, -0.020905},
    {-0.047064, -0.085629, -0.080677},
    {-0.067523, -0.128972, -0.119538},
    {0.186086, -0.016828, 0.070014},
    {0.187364, 0.017133, 0.075949},
    {-0.112669, -0.037433, -0.298944},
    {-0.068276, -0.114504, -0.265795},
    {0.147510, -0.040616, -0.013687},
    {0.133084, -0.062849, -0.032637},
    {-0.416571, -0.041544, -0.125088},
    {-0.505337, -0.044193, -0.157651},
    {-0.154132, -0.075106, 0.050466},
    {-0.148036, -0.059719, 0.121516},
    {0.490555, 0.157659, -0.222208},
    {0.436700, 0.120500, -0.205869},
    {0.754525, 0.269323, 0.045810},
    {0.645077, 0.271923, 0.013942},
    {0.237023, 0.115337, -0.026429},
    {0.204895, 0.121020, -0.008541},
    {0.383999, 0.153963, 0.171763},
    {0.385026, 0.222074, 0.239731},
    {0.198232, 0.072972, -0.108179},
    {0.147882, 0.074743, -0.123341},
    {0.390929, 0.075205, 0.081828},
    {0.341623, 0.089405, 0.069389},
    {-0.003381, 0.159694, -0.016026},
    {-0.043653, 0.206860, -0.040729},
    {0.135515, 0.107824, 0.179310},
    {0.081086, 0.119673, 0.174282},
    {0.192637, 0.400335, -0.341906},
    {0.171196, 0.284921, -0.221516},
    {0.377807, 0.359087, -0.151523},
    {0.411052, 0.297925, -0.099774},
    {-0.010060, 0.261887, -0.149567},
    {-0.107877, 0.287756, -0.116982},
    {0.158003, 0.209727, 0.077988},
    {0.109710, 0.232272, 0.088135},
    {0.000698, 0.209353, -0.395208},
    {-0.094015, 0.230322, -0.279928},
    {0.137355, 0.230881, -0.124115},
    {0.103058, 0.166855, -0.100386},
    {-0.305058, 0.305422, -0.176026},
    {-0.422049, 0.337137, -0.293297},
    {-0.121744, 0.185124, 0.048115},
    {-0.171052, 0.200312, 0.052812},
    {0.224091, -0.010673, -0.019727},
    {0.200266, -0.020167, 0.001798},
    {0.382742, 0.032362, 0.161665},
    {0.345631, -0.019705, 0.164451},
    {0.029431, 0.045010, 0.071518},
    {0.031940, 0.010876, 0.087037},
    {0.181935, 0.039112, 0.202316},
    {0.181810, 0.033189, 0.253435},
    {-0.008677, -0.066679, -0.144737},
    {-0.021768, -0.021288, -0.125903},
    {0.136766, 0.000100, 0.059449},
    {0.135405, -0.020446, 0.103793},
    {-0.289115, 0.039747, -0.012256},
    {-0.338683, 0.025909, -0.034058},
    {-0.016515, 0.048584, 0.197981},
    {-0.046790, 0.011816, 0.199964},
    {0.094214, 0.127422, -0.169936},
    {0.048279, 0.096189, -0.148153},
    {0.217391, 0.081732, 0.013677},
    {0.179656, 0.084671, 0.031434},
    {-0.227367, 0.118176, -0.039803},
    {-0.327096, 0.159747, -0.018931},
    {0.000834, 0.113118, 0.125325},
    {-0.014617, 0.128924, 0.163776},
    {-0.254570, 0.154329, -0.232018},
    {-0.353068, 0.124341, -0.174409},
    {-0.061004, 0.107744, 0.037257},
    {-0.100991, 0.080302, 0.062701},
    {-0.927022, 0.285660, -0.240549},
    {-1.153224, 0.277232, -0.322538},
    {-0.569012, 0.108135, 0.172634},
    {-0.555273, 0.131461, 0.325930},
    {0.518847, 0.065683, -0.132877},
    {0.501324, -0.006585, -0.094884},
    {1.066190, -0.150380, 0.201791},
    {0.858377, -0.166415, 0.081686},
    {0.320584, -0.031499, 0.039534},
    {0.311442, -0.075120, 0.026013},
    {0.625829, -0.019856, 0.346041},
    {0.525271, -0.003948, 0.284868},
    {0.312594, -0.075673, -0.066642},
    {0.295732, -0.057895, -0.042207},
    {0.550446, -0.029110, 0.046850},
    {0.465467, -0.068987, 0.096167},
    {0.122669, -0.051786, 0.044283},
    {0.079669, -0.044145, 0.045805},
    {0.238778, -0.031835, 0.171694},
    {0.200734, -0.072619, 0.178726},
    {0.342512, 0.131270, -0.163021},
    {0.294028, 0.111759, -0.125793},
    {0.589523, 0.121808, -0.049372},
    {0.550506, 0.132318, 0.017485},
    {0.164280, 0.047560, -0.058383},
    {0.120110, 0.049242, -0.052403},
    {0.269181, 0.035000, 0.103494},
    {0.297466, 0.038517, 0.139289},
    {0.094549, -0.030880, -0.153376},
    {0.080363, 0.024359, -0.127578},
    {0.281351, 0.055178, 0.000155},
    {0.234900, 0.039477, 0.013957},
    {-0.118161, 0.011976, -0.034270},
    {-0.157654, 0.027765, -0.005010},
    {0.102631, 0.027283, 0.099723},
    {0.077285, 0.052532, 0.115583},
    {0.329398, -0.278552, 0.016316},
    {0.305993, -0.267896, 0.094952},
    {0.775270, -0.394995, 0.290748},
    {0.583180, -0.252159, 0.285391},
    {0.192226, -0.182242, 0.126859},
    {0.185908, -0.245779, 0.159940},
    {0.346293, -0.250404, 0.355682},
    {0.354160, -0.364521, 0.472337},
    {0.134942, -0.313666, -0.115181},
    {0.126077, -0.286568, -0.039927},
    {0.405618, -0.211792, 0.199095},
    {0.312099, -0.213642, 0.190972},
    {-0.071392, -0.297366, 0.081426},
    {-0.165839, -0.301986, 0.160640},
    {0.147808, -0.290712, 0.298198},
    {0.063302, -0.310149, 0.396302},
    {0.141444, -0.081377, -0.076621},
    {0.115936, -0.104440, -0.039885},
    {0.367023, -0.087281, 0.096390},
    {0.330038, -0.117958, 0.127050},
    {0.002897, -0.062454, 0.025151},
    {-0.052404, -0.082200, 0.041975},
    {0.181553, -0.137004, 0.230489},
    {0.140768, -0.094604, 0.265928},
    {-0.101763, -0.209566, -0.135964},
    {-0.159056, -0.191005, -0.095509},
    {0.045016, -0.081562, 0.075942},
    {0.016808, -0.112482, 0.068593},
    {-0.408578, -0.132377, 0.079163},
    {-0.431534, -0.214646, 0.157714},
    {-0.096931, -0.101938, 0.200304},
    {-0.167867, -0.114851, 0.262964},
    {0.393882, 0.086002, 0.008961},
    {0.338747, 0.048405, -0.004187},
    {0.877844, 0.374373, 0.171008},
    {0.740790, 0.324525, 0.242248},
    {0.200218, 0.070150, 0.085891},
    {0.171760, 0.090531, 0.102579},
    {0.314263, 0.126417, 0.322833},
    {0.313523, 0.065445, 0.403855},
    {0.164261, 0.057745, -0.005490},
    {0.122141, 0.024122, 0.009190},
    {0.308248, 0.078401, 0.180577},
    {0.251222, 0.073868, 0.160457},
    {-0.047526, 0.023725, 0.086336},
    {-0.091643, 0.005539, 0.093179},
    {0.079339, 0.044135, 0.206697},
    {0.104213, 0.011277, 0.240060},
    {0.226607, 0.186234, -0.056881},
    {0.173281, 0.158131, -0.059413},
    {0.339400, 0.214501, 0.052905},
    {0.309166, 0.188181, 0.058028},
    {0.014442, 0.194715, 0.048945},
    {-0.028793, 0.194766, 0.089078},
    {0.069564, 0.206743, 0.193568},
    {0.091532, 0.202786, 0.269680},
    {-0.071196, 0.135604, -0.103744},
    {-0.118288, 0.152837, -0.060151},
    {0.146856, 0.143174, 0.061789},
    {0.104379, 0.143672, 0.056797},
    {-0.541832, 0.250034, -0.017602},
    {-0.641583, 0.278411, -0.111909},
    {-0.094447, 0.159393, 0.164848},
    {-0.113612, 0.120702, 0.221656},
    {0.204918, -0.078894, 0.075524},
    {0.161232, -0.090256, 0.088701},
    {0.378460, -0.033687, 0.309964},
    {0.311701, -0.049984, 0.316881},
    {0.019311, -0.050048, 0.212387},
    {0.002473, -0.062855, 0.278462},
    {0.151448, -0.090652, 0.410031},
    {0.162778, -0.071291, 0.531252},
    {-0.083704, -0.076839, -0.020798},
    {-0.092832, -0.043492, 0.029202},
    {0.136844, -0.077791, 0.186493},
    {0.089536, -0.086826, 0.184711},
    {-0.270255, -0.058858, 0.173048},
    {-0.350416, -0.009219, 0.273260},
    {-0.105248, -0.205534, 0.425159},
    {-0.135030, -0.197464, 0.623550},
    {-0.051717, 0.069756, -0.043829},
    {-0.081050, 0.056947, -0.000205},
    {0.190388, 0.016366, 0.145922},
    {0.142662, 0.002575, 0.159182},
    {-0.352890, 0.011117, 0.091040},
    {-0.367374, 0.056547, 0.147209},
    {-0.003179, 0.026570, 0.282541},
    {-0.069934, -0.005171, 0.337678},
    {-0.496181, 0.026464, 0.019432},
    {-0.690384, 0.069313, -0.004175},
    {-0.146138, 0.046372, 0.161839},
    {-0.197581, 0.034093, 0.241003},
    {-0.989567, 0.040993, 0.049384},
    {-1.151075, 0.210556, 0.237374},
    {-0.335366, -0.058208, 0.480168},
    {-0.502419, -0.093761, 0.675240},
    {0.862548, 0.264137, -0.294905},
    {0.782668, 0.251324, -0.122108},
    {1.597797, 0.463818, -0.133153},
    {1.615756, 0.060653, 0.084764},
    {0.435588, 0.209832, 0.095050},
    {0.431013, 0.165328, 0.047909},
    {1.248164, 0.265923, 0.488086},
    {1.009933, 0.345440, 0.473702},
    {0.477017, 0.194237, -0.058012},
    {0.401362, 0.186915, -0.054137},
    {1.202158, 0.284782, -0.066531},
    {1.064907, 0.203766, 0.046383},
    {0.255848, 0.133398, 0.046049},
    {0.218680, 0.128833, 0.065326},
    {0.490817, 0.182041, 0.286583},
    {0.440714, 0.106576, 0.301120},
    {0.604263, 0.522925, -0.238629},
    {0.526329, 0.377577, -0.198100},
    {1.038632, 0.606242, -0.121253},
    {0.995283, 0.552202, 0.110700},
    {0.262232, 0.313664, -0.086909},
    {0.230835, 0.273385, -0.054268},
    {0.548466, 0.490721, 0.278201},
    {0.466984, 0.355859, 0.289160},
    {0.367137, 0.236160, -0.228114},
    {0.309359, 0.233843, -0.171325},
    {0.465268, 0.276569, 0.010951},
    {0.378124, 0.250237, 0.011131},
    {0.061885, 0.296810, -0.011420},
    {0.000125, 0.350029, -0.011277},
    {0.163815, 0.261191, 0.175863},
    {0.165132, 0.308797, 0.227800},
    {0.461418, 0.052075, -0.016543},
    {0.472372, 0.046962, 0.045746},
    {0.856406, 0.136415, 0.245074},
    {0.834616, 0.003254, 0.372643},
    {0.337869, 0.036994, 0.232513},
    {0.267414, 0.027593, 0.252779},
    {0.584983, 0.113046, 0.583119},
    {0.475406, -0.024234, 0.655070},
    {0.264823, -0.029292, 0.004270},
    {0.246071, -0.019109, 0.030048},
    {0.477401, 0.021039, 0.155448},
    {0.458453, -0.043959, 0.187850},
    {0.067059, -0.061227, 0.126904},
    {0.044608, -0.034575, 0.150205},
    {0.191304, -0.003810, 0.316776},
    {0.153078, 0.029915, 0.361303},
    {0.320704, 0.178950, -0.088835},
    {0.300866, 0.137645, -0.056893},
    {0.553442, 0.162339, 0.131987},
    {0.490083, 0.123682, 0.146163},
    {0.118950, 0.083109, 0.034052},
    {0.099344, 0.066212, 0.054329},
    {0.228325, 0.122445, 0.309219},
    {0.172093, 0.135754, 0.323361},
    {0.064213, 0.063405, -0.058243},
    {0.011906, 0.088795, -0.069678},
    {0.194232, 0.129185, 0.125708},
    {0.155182, 0.174013, 0.144099},
    {-0.217068, 0.112731, 0.093497},
    {-0.307590, 0.171146, 0.110735},
    {-0.014897, 0.138094, 0.232455},
    {-0.036936, 0.170135, 0.279166},
    {0.681886, 0.437121, 0.078458},
    {0.548559, 0.376914, 0.092485},
    {1.259194, 0.901494, 0.256085},
    {1.296139, 0.607949, 0.302184},
    {0.319619, 0.307231, 0.099647},
    {0.287232, 0.359355, 0.186844},
    {0.751306, 0.676688, 0.499386},
    {0.479609, 0.553030, 0.560447},
    {0.276377, 0.214032, -0.003661},
    {0.238146, 0.223595, 0.028806},
    {0.542688, 0.266205, 0.171393},
    {0.460188, 0.283979, 0.158288},
    {0.057385, 0.309853, 0.144517},
    {-0.006881, 0.348152, 0.097310},
    {0.244434, 0.247298, 0.322601},
    {0.253992, 0.335420, 0.402241},
    {0.354006, 0.579776, -0.130176},
    {0.267043, 0.461976, -0.058178},
    {0.534049, 0.626549, 0.046747},
    {0.441835, 0.468260, 0.057556},
    {0.110477, 0.628795, 0.102950},
    {0.031409, 0.489068, 0.090605},
    {0.229564, 0.525640, 0.325454},
    {0.105570, 0.582151, 0.509738},
    {0.005690, 0.521474, -0.157885},
    {0.104463, 0.424022, -0.080647},
    {0.223784, 0.389860, 0.060904},
    {0.159806, 0.340571, 0.062061},
    {-0.173976, 0.573425, 0.027383},
    {-0.376008, 0.587868, 0.133042},
    {-0.051773, 0.348339, 0.231923},
    {-0.122571, 0.473049, 0.251159},
    {0.324321, 0.148510, 0.116006},
    {0.282263, 0.121730, 0.114016},
    {0.690108, 0.256346, 0.418128},
    {0.542523, 0.294427, 0.461973},
    {0.056944, 0.107667, 0.281797},
    {0.027844, 0.106858, 0.355071},
    {0.160456, 0.177656, 0.528819},
    {0.227537, 0.177976, 0.689465},
    {0.111585, 0.097896, 0.109244},
    {0.083994, 0.133245, 0.115789},
    {0.208740, 0.142084, 0.208953},
    {0.156072, 0.143303, 0.231368},
    {-0.185830, 0.214347, 0.309774},
    {-0.311053, 0.240517, 0.328512},
    {-0.041749, 0.090901, 0.511373},
    {-0.156164, 0.098486, 0.478020},
    {0.151543, 0.263073, -0.033471},
    {0.126322, 0.213004, -0.007014},
    {0.245313, 0.217564, 0.120210},
    {0.259136, 0.225542, 0.176601},
    {-0.190632, 0.260214, 0.141755},
    {-0.189271, 0.331768, 0.170606},
    {0.054763, 0.294766, 0.357775},
    {-0.033724, 0.257645, 0.365069},
    {-0.184971, 0.396532, 0.057728},
    {-0.293313, 0.400259, 0.001123},
    {-0.015219, 0.232287, 0.177913},
    {-0.022524, 0.244724, 0.240753},
    {-0.520342, 0.347950, 0.249265},
    {-0.671997, 0.410782, 0.153434},
    {-0.253089, 0.412356, 0.489854},
    {-0.410922, 0.562454, 0.543891}
};

/*
 * PRBA58 Vector Quantizer Levels
 */
const float AmbePRBA58[128][4] = {
    {-0.103660, 0.094597, -0.013149, 0.081501},
    {-0.170709, 0.129958, -0.057316, 0.112324},
    {-0.095113, 0.080892, -0.027554, 0.003371},
    {-0.154153, 0.113437, -0.074522, 0.003446},
    {-0.109553, 0.153519, 0.006858, 0.040930},
    {-0.181931, 0.217882, -0.019042, 0.040049},
    {-0.096246, 0.144191, -0.024147, -0.035120},
    {-0.174811, 0.193357, -0.054261, -0.071700},
    {-0.183241, -0.052840, 0.117923, 0.030960},
    {-0.242634, 0.009075, 0.098007, 0.091643},
    {-0.143847, -0.028529, 0.040171, -0.002812},
    {-0.198809, 0.006990, 0.020668, 0.026641},
    {-0.233172, -0.028793, 0.140130, -0.071927},
    {-0.309313, 0.056873, 0.108262, -0.018930},
    {-0.172782, -0.002037, 0.048755, -0.087065},
    {-0.242901, 0.036076, 0.015064, -0.064366},
    {0.077107, 0.172685, 0.159939, 0.097456},
    {0.024820, 0.209676, 0.087347, 0.105204},
    {0.085113, 0.151639, 0.084272, 0.022747},
    {0.047975, 0.196695, 0.038770, 0.029953},
    {0.113925, 0.236813, 0.176121, 0.016635},
    {0.009708, 0.267969, 0.127660, 0.015872},
    {0.114044, 0.202311, 0.096892, -0.043071},
    {0.047219, 0.260395, 0.050952, -0.046996},
    {-0.055095, 0.034041, 0.200464, 0.039050},
    {-0.061582, 0.069566, 0.113048, 0.027511},
    {-0.025469, 0.040440, 0.132777, -0.039098},
    {-0.031388, 0.064010, 0.067559, -0.017117},
    {-0.074386, 0.086579, 0.228232, -0.055461},
    {-0.107352, 0.120874, 0.137364, -0.030252},
    {-0.036897, 0.089972, 0.155831, -0.128475},
    {-0.059070, 0.097879, 0.084489, -0.075821},
    {-0.050865, -0.025167, -0.086636, 0.011256},
    {-0.051426, 0.013301, -0.144665, 0.038541},
    {-0.073831, -0.028917, -0.142416, -0.025268},
    {-0.083910, 0.015004, -0.227113, -0.002808},
    {-0.030840, -0.009326, -0.070517, -0.041304},
    {-0.022018, 0.029381, -0.124961, -0.031624},
    {-0.064222, -0.014640, -0.108798, -0.092342},
    {-0.038801, 0.038133, -0.188992, -0.094221},
    {-0.154059, -0.183932, -0.019894, 0.082105},
    {-0.188022, -0.113072, -0.117380, 0.090911},
    {-0.243301, -0.207086, -0.053735, -0.001975},
    {-0.275931, -0.121035, -0.161261, 0.004231},
    {-0.118142, -0.157537, -0.036594, -0.008679},
    {-0.153627, -0.111372, -0.103095, -0.009460},
    {-0.173458, -0.180158, -0.057130, -0.103198},
    {-0.208509, -0.127679, -0.149336, -0.109289},
    {0.096310, 0.047927, -0.024094, -0.057018},
    {0.044289, 0.075486, -0.008505, -0.067635},
    {0.076751, 0.025560, -0.066428, -0.102991},
    {0.025215, 0.090417, -0.058616, -0.114284},
    {0.125980, 0.070078, 0.016282, -0.112355},
    {0.070859, 0.118988, 0.001180, -0.116359},
    {0.097520, 0.059219, -0.026821, -0.172850},
    {0.048226, 0.145459, -0.050093, -0.188853},
    {0.007242, -0.135796, 0.147832, -0.034080},
    {0.012843, -0.069616, 0.077139, -0.047909},
    {-0.050911, -0.116323, 0.082521, -0.056362},
    {-0.039630, -0.055678, 0.036066, -0.067992},
    {0.042694, -0.091527, 0.150940, -0.124225},
    {0.029225, -0.039401, 0.071664, -0.113665},
    {-0.025085, -0.099013, 0.074622, -0.138674},
    {-0.031220, -0.035717, 0.020870, -0.143376},
    {0.040638, 0.087903, -0.049500, 0.094607},
    {0.026860, 0.125924, -0.103449, 0.140882},
    {0.075166, 0.110186, -0.115173, 0.067330},
    {0.036642, 0.163193, -0.188762, 0.103724},
    {0.028179, 0.095124, -0.053258, 0.028900},
    {0.002307, 0.148211, -0.096037, 0.046189},
    {0.072227, 0.137595, -0.095629, 0.001339},
    {0.033308, 0.221480, -0.152201, 0.012125},
    {0.003458, -0.085112, 0.041850, 0.113836},
    {-0.040610, -0.044880, 0.029732, 0.177011},
    {0.011404, -0.054324, -0.012426, 0.077815},
    {-0.042413, -0.030930, -0.034844, 0.122946},
    {-0.002206, -0.045698, 0.050651, 0.054886},
    {-0.041729, -0.016110, 0.048005, 0.102125},
    {0.013963, -0.022204, 0.001613, 0.028997},
    {-0.030218, -0.002052, -0.004365, 0.065343},
    {0.299049, 0.046260, 0.076320, 0.070784},
    {0.250160, 0.098440, 0.012590, 0.137479},
    {0.254170, 0.095310, 0.018749, 0.004288},
    {0.218892, 0.145554, -0.035161, 0.069784},
    {0.303486, 0.101424, 0.135996, -0.013096},
    {0.262919, 0.165133, 0.077237, 0.071721},
    {0.319358, 0.170283, 0.054554, -0.072210},
    {0.272983, 0.231181, -0.014471, 0.011689},
    {0.134116, -0.026693, 0.161400, 0.110292},
    {0.100379, 0.026517, 0.086236, 0.130478},
    {0.144718, -0.000895, 0.093767, 0.044514},
    {0.114943, 0.022145, 0.035871, 0.069193},
    {0.122051, 0.011043, 0.192803, 0.022796},
    {0.079482, 0.026156, 0.117725, 0.056565},
    {0.124641, 0.027387, 0.122956, -0.025369},
    {0.090708, 0.027357, 0.064450, 0.013058},
    {0.159781, -0.055202, -0.090597, 0.151598},
    {0.084577, -0.037203, -0.126698, 0.119739},
    {0.192484, -0.100195, -0.162066, 0.104148},
    {0.114579, -0.046270, -0.219547, 0.100067},
    {0.153083, -0.010127, -0.086266, 0.068648},
    {0.088202, -0.010515, -0.102196, 0.046281},
    {0.164494, -0.057325, -0.132860, 0.024093},
    {0.109419, -0.013999, -0.169596, 0.020412},
    {0.039180, -0.209168, -0.035872, 0.087949},
    {0.012790, -0.177723, -0.129986, 0.073364},
    {0.045261, -0.256694, -0.088186, 0.004212},
    {-0.005314, -0.231202, -0.191671, -0.002628},
    {0.037963, -0.153227, -0.045364, 0.003322},
    {0.030800, -0.126452, -0.114266, -0.010414},
    {0.044125, -0.184146, -0.081400, -0.077341},
    {0.029204, -0.157393, -0.172017, -0.089814},
    {0.393519, -0.043228, -0.111365, -0.000740},
    {0.289581, 0.018928, -0.123140, 0.000713},
    {0.311229, -0.059735, -0.198982, -0.081664},
    {0.258659, 0.052505, -0.211913, -0.034928},
    {0.300693, 0.011381, -0.083545, -0.086683},
    {0.214523, 0.053878, -0.101199, -0.061018},
    {0.253422, 0.028496, -0.156752, -0.163342},
    {0.199123, 0.113877, -0.166220, -0.102584},
    {0.249134, -0.165135, 0.028917, 0.051838},
    {0.156434, -0.123708, 0.017053, 0.043043},
    {0.214763, -0.101243, -0.005581, -0.020703},
    {0.140554, -0.072067, -0.015063, -0.011165},
    {0.241791, -0.152048, 0.106403, -0.046857},
    {0.142316, -0.131899, 0.054076, -0.026485},
    {0.206535, -0.086116, 0.046640, -0.097615},
    {0.129759, -0.081874, 0.004693, -0.073169}
};

/*
 * Higher Order Coefficients
 */
const float AmbeHOCb5[32][4] = {
    {0.264108, 0.045976, -0.200999, -0.122344},
    {0.479006, 0.227924, -0.016114, -0.006835},
    {0.077297, 0.080775, -0.068936, 0.041733},
    {0.185486, 0.231840, 0.182410, 0.101613},
    {-0.012442, 0.223718, -0.277803, -0.034370},
    {-0.059507, 0.139621, -0.024708, -0.104205},
    {-0.248676, 0.255502, -0.134894, -0.058338},
    {-0.055122, 0.427253, 0.025059, -0.045051},
    {-0.058898, -0.061945, 0.028030, -0.022242},
    {0.084153, 0.025327, 0.066780, -0.180839},
    {-0.193125, -0.082632, 0.140899, -0.089559},
    {0.000000, 0.033758, 0.276623, 0.002493},
    {-0.396582, -0.049543, -0.118100, -0.208305},
    {-0.287112, 0.096620, 0.049650, -0.079312},
    {-0.543760, 0.171107, -0.062173, -0.010483},
    {-0.353572, 0.227440, 0.230128, -0.032089},
    {0.248579, -0.279824, -0.209589, 0.070903},
    {0.377604, -0.119639, 0.008463, -0.005589},
    {0.102127, -0.093666, -0.061325, 0.052082},
    {0.154134, -0.105724, 0.099317, 0.187972},
    {-0.139232, -0.091146, -0.275479, -0.038435},
    {-0.144169, 0.034314, -0.030840, 0.022207},
    {-0.143985, 0.079414, -0.194701, 0.175312},
    {-0.195329, 0.087467, 0.067711, 0.186783},
    {-0.123515, -0.377873, -0.209929, -0.212677},
    {0.068698, -0.255933, 0.120463, -0.095629},
    {-0.106810, -0.319964, -0.089322, 0.106947},
    {-0.158605, -0.309606, 0.190900, 0.089340},
    {-0.489162, -0.432784, -0.151215, -0.005786},
    {-0.370883, -0.154342, -0.022545, 0.114054},
    {-0.742866, -0.204364, -0.123865, -0.038888},
    {-0.573077, -0.115287, 0.208879, -0.027698}
};

/*
 * Higher Order Coefficients
 */
const float AmbeHOCb6[16][4] = {
    {-0.143886, 0.235528, -0.116707, 0.025541},
    {-0.170182, -0.063822, -0.096934, 0.109704},
    {0.232915, 0.269793, 0.047064, -0.032761},
    {0.153458, 0.068130, -0.033513, 0.126553},
    {-0.440712, 0.132952, 0.081378, -0.013210},
    {-0.480433, -0.249687, -0.012280, 0.007112},
    {-0.088001, 0.167609, 0.148323, -0.119892},
    {-0.104628, 0.102639, 0.183560, 0.121674},
    {0.047408, -0.000908, -0.214196, -0.109372},
    {0.113418, -0.240340, -0.121420, 0.041117},
    {0.385609, 0.042913, -0.184584, -0.017851},
    {0.453830, -0.180745, 0.050455, 0.030984},
    {-0.155984, -0.144212, 0.018226, -0.146356},
    {-0.104028, -0.260377, 0.146472, 0.101389},
    {0.012376, -0.000267, 0.006657, -0.013941},
    {0.165852, -0.103467, 0.119713, -0.075455}
};

/*
 * Higher Order Coefficients
 */
const float AmbeHOCb7[16][4] = {
    {0.182478, 0.271794, -0.057639, 0.026115},
    {0.110795, 0.092854, 0.078125, -0.082726},
    {0.057964, 0.000833, 0.176048, 0.135404},
    {-0.027315, 0.098668, -0.065801, 0.116421},
    {-0.222796, 0.062967, 0.201740, -0.089975},
    {-0.193571, 0.309225, -0.014101, -0.034574},
    {-0.389053, -0.181476, 0.107682, 0.050169},
    {-0.345604, 0.064900, -0.065014, 0.065642},
    {0.319393, -0.055491, -0.220727, -0.067499},
    {0.460572, 0.084686, 0.048453, -0.011050},
    {0.201623, -0.068994, -0.067101, 0.108320},
    {0.227528, -0.173900, 0.092417, -0.066515},
    {-0.016927, 0.047757, -0.177686, -0.102163},
    {-0.052553, -0.065689, 0.019328, -0.033060},
    {-0.144910, -0.238617, -0.195206, -0.063917},
    {-0.024159, -0.338822, 0.003581, 0.060995}
};

/*
 * Higher Order Coefficients
 */
const float AmbeHOCb8[8][4] = {
    {0.323968, 0.008964, -0.063117, 0.027909},
    {0.010900, -0.004030, -0.125016, -0.080818},
    {0.109969, 0.256272, 0.042470, 0.000749},
    {-0.135446, 0.201769, -0.083426, 0.093888},
    {-0.441995, 0.038159, 0.022784, 0.003943},
    {-0.155951, 0.032467, 0.145309, -0.041725},
    {-0.149182, -0.223356, -0.065793, 0.075016},
    {0.096949, -0.096400, 0.083194, 0.049306}
};

#endif // __AMBE3600x2450_CONST_H__
